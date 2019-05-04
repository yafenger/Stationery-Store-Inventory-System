using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADProjectBase2.Models;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;

namespace ADProjectBase2.Controllers
{
    public class DelegationsController : Controller
    {
        private readonly SSISContext _context;

        private readonly UserManager<ApplicationUser> _userManager;
        public DelegationsController(SSISContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // GET: Starting view for assign or delete delegation.
        public IActionResult changeOrAddDelegation(string searchString)
        {
            if (HttpContext.User.IsInRole("head"))
            {
                MyUser head = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int? id = head.DeptId;
                Delegation delegation = _context.Delegation.Where(m => m.DeptId == id).ToList().FirstOrDefault();

                ViewData["delegation"] = delegation;
                ViewData["CurrentFilter"] = searchString;
                var employees = _context.MyUser.Where(u => u.DeptId == id &&(u.RoleId == 1|| u.RoleId==6));
                if (!String.IsNullOrEmpty(searchString))
                {
                    employees = employees.Where(e => e.Name.Contains(searchString));
                }
                List<MyUser> ulist = employees.ToList();
                ViewData["User"] = ulist;

                if (delegation == null)
                {
                    return View(new Delegation());
                }
                else
                {
                    return View(delegation);
                }
            }
            else return NotFound();
        }

        // POST: deal with the change of the delegation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> changeOrAddDelegation(string SubmitButton, string searchString, Delegation delegation)
        {                        
            MyUser head = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
            int id = head.DeptId;

            if (ModelState.IsValid)
            {
                try
                {
                    Delegation de = _context.Delegation.Where(m => m.DeptId == id).FirstOrDefault();
                    MyUser u;
                    if (de == null)
                    {
                        Delegation d = new Delegation();
                        d.DeptId = id;
                        d.UserId = delegation.UserId;
                        d.Startdate = delegation.Startdate;
                        d.Enddate = delegation.Enddate;
                        _context.Add(d);
                        await _context.SaveChangesAsync();
                        de = _context.Delegation.Where(m => m.DeptId == id).FirstOrDefault();
                    } 
                    
                    else
                    {
                        
                        string buttonClicked = SubmitButton;
                        if (buttonClicked == "Cancel")
                        {
                            u = _context.MyUser.Where(em => em.RoleId == 6 & em.DeptId == id).ToList().FirstOrDefault();
                            if (u != null)
                            {
                                u.RoleId = 1;
                                _context.Update(u);
                                ApplicationUser _user1 = await _userManager.FindByEmailAsync(u.Email);
                                await _userManager.RemoveFromRoleAsync(_user1, "actinghead");
                                await _userManager.AddToRoleAsync(_user1, "employee");
                            }
                            _context.Remove(de);

                            await _context.SaveChangesAsync();
                            return RedirectToAction("About", "Home");
                            //after cancel,directly return,no need to do the next step since have already changed the role.
                        }
                        else
                        {
                            de.DeptId = id;
                            de.UserId = delegation.UserId;
                            de.Startdate = delegation.Startdate;
                            de.Enddate = delegation.Enddate;
                            _context.Update(de);
                            await _context.SaveChangesAsync();
                        }
                    }
                    //Deal with the delegation(null or not null) first,then deal with the role all together at last.
                    u = _context.MyUser.Where(em => em.RoleId == 6 & em.DeptId == id).FirstOrDefault();
                    if (u != null)
                    {
                        u.RoleId = 1;
                        _context.Update(u);
                        //*
                        ApplicationUser _user1 = await _userManager.FindByEmailAsync(u.Email);
                        await _userManager.RemoveFromRoleAsync(_user1, "actinghead");
                        await _userManager.AddToRoleAsync(_user1, "employee");
                    }
                    int eid = await _context.Delegation.Where(d => d.DeptId == id).Select(d => d.UserId).SingleAsync();
                    MyUser e = _context.MyUser.Find(eid);
                    e.RoleId = 6;
                    _context.Update(e);
                    //*
                    ApplicationUser _user = await _userManager.FindByEmailAsync(e.Email);
                    await _userManager.RemoveFromRoleAsync(_user, "employee");
                    await _userManager.AddToRoleAsync(_user, "actinghead");

                    await _context.SaveChangesAsync();

                    #region Email to remind the delegation within the department.
                    List<MyUser> alldepemployees = new List<MyUser>();

                    alldepemployees = _context.MyUser.Where(x => x.DeptId == id).ToList();
                    DateTime startdate = delegation.Startdate.Value;
                    String stringstartdate = startdate.ToShortDateString();
                    DateTime enddate = delegation.Enddate.Value;
                    String stringenddate = enddate.ToShortDateString();
                    MyUser actinghead = new MyUser();
                    actinghead = _context.MyUser.Where(x => x.DeptId == id && x.RoleId == 6).FirstOrDefault();
                    Department department = _context.Department.Where(x => x.DeptId == id).FirstOrDefault();

                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("SSIS", "team2adproject@gmail.com"));
                    foreach (MyUser e1 in alldepemployees)
                    {
                        message.To.Add(new MailboxAddress(e1.Name, e1.Email));
                        message.Subject = "New department actinghead";
                        message.Body = new TextPart("plain")
                        {
                            Text = "Dear " + e1.Name + "," +
                            "I will be on leave from " + stringstartdate + " till " + stringenddate + "." + " I may not be able to respond to the emails or requests"
                            + " due to limited email access overseas. During this period, " + actinghead.Name + " will be the acting head of" +
                            " our department to be in charge of all matters regarding to stationery. " +
                            Environment.NewLine +
                            "This is an automatically generated email. Please do not reply." +
                            Environment.NewLine +
                            "Regards" +
                            Environment.NewLine +
                            department.Name + " department"

                        };

                    }
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, false);
                        client.Authenticate("team2adproject@gmail.com", "team2team2");
                        client.Send(message);
                        client.Disconnect(true);
                    }
                    #endregion

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DelegationExists(delegation.DeptId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("About", "Home");
            }

            ViewData["CurrentFilter"] = searchString;
            var employees = _context.MyUser.Where(u => u.DeptId == id &&(u.RoleId == 1|| u.RoleId==6));
            if (!String.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(x => x.Name.Contains(searchString));
            }
            List<MyUser> ulist = employees.ToList();
            ViewData["User"] = ulist;
            return View(delegation);
        }


        private bool DelegationExists(int id)
        {
            return _context.Delegation.Any(e => e.DelegationId == id);
        }
    }
}
