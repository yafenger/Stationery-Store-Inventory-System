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
    public class DepartmentsController : Controller
    {
        private readonly SSISContext _context;

        private readonly UserManager<ApplicationUser> _userManager;
        public DepartmentsController(SSISContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        public ActionResult dutyoff()
        {
            if (HttpContext.User.IsInRole("head"))
            {
                return View();
            }
            else return NotFound();
        }
        public ActionResult expired()
        {
            if (HttpContext.User.IsInRole("actinghead"))
            {
                return View();
            }
            else return NotFound();
        }


        // GET: display all the collection points
        public async Task<IActionResult> changeCollectionPoint()
        {
            if (HttpContext.User.Identity.IsAuthenticated&&(HttpContext.User.IsInRole("head")|| HttpContext.User.IsInRole("actinghead")|| HttpContext.User.IsInRole("representative")))
            {
                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int? id = user.DeptId;
                Delegation deleg = await (from m in _context.Delegation.Where(p => p.DeptId == id) select m).FirstOrDefaultAsync();
                if (deleg != null && HttpContext.User.IsInRole("head"))
                {
                    
                    if (DateTime.Compare((DateTime)deleg.Startdate, DateTime.Now) < 0 && DateTime.Compare((DateTime)deleg.Enddate, DateTime.Now) > 0)
                    {
                        return Redirect("~/Departments/dutyoff");
                    }
                }
                else if (deleg != null && HttpContext.User.IsInRole("actinghead"))
                {
                    if (DateTime.Compare((DateTime)deleg.Startdate, DateTime.Now) > 0 || DateTime.Compare((DateTime)deleg.Enddate, DateTime.Now) < 0)
                    {
                        user.RoleId = 1;
                        _context.Update(user);

                        ApplicationUser _user1 = await _userManager.FindByEmailAsync(user.Email);
                        await _userManager.RemoveFromRoleAsync(_user1, "actinghead");
                        await _userManager.AddToRoleAsync(_user1, "employee");

                        return Redirect("~/Departments/expired");
                    }
                }
                if (id == null)
                {
                    return NotFound();
                }

                var department = await _context.Department.SingleOrDefaultAsync(m => m.DeptId == id);
                if (department == null)
                {
                    return NotFound();
                }

                List<CollectionPoint> cp = _context.CollectionPoint.ToList();
                ViewData["Cpid"] = cp;

                return View(department);
            }
            else return NotFound();
        }

        // POST: change collection point
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> changeCollectionPoint([Bind("DeptId,Name,DeptRep,Cpid")] Department department,CollectionPoint collectionPoint)
        {

                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int? id = user.DeptId;
                if (id != department.DeptId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        _context.Update(department);
                        await _context.SaveChangesAsync();
                        
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!DepartmentExists(department.DeptId))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    List<MyUser> allclerks = new List<MyUser>();
                    allclerks = _context.MyUser.Where(x => x.RoleId == 5).ToList();
                    CollectionPoint cp = new CollectionPoint();
                    cp = _context.CollectionPoint.Where(x => x.Cpid == collectionPoint.Cpid).FirstOrDefault();

                    #region Email
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("SSIS", "team2adproject@gmail.com"));
                    //message.To.Add(new MailboxAddress("tqy", "tanqiyang@gmail.com"));
                    message.Subject = "New collection point";
                    foreach (MyUser clerk in allclerks)
                    {
                        message.To.Add(new MailboxAddress(clerk.Name, clerk.Email));

                        message.Body = new TextPart("plain")
                        {
                            Text = "Dear " + clerk.Name + ", " +
                            "This letter is to let you know we have selected the collection point " +
                            "and delivery timing as " + cp.Name + "." +
                            "Please check and find details of the location below:" +
                            Environment.NewLine +
                            cp.Details +
                            Environment.NewLine +
                            "Regards" +
                            Environment.NewLine +
                            department.Name +
                            " department"
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
                    return RedirectToAction("About", "Home");
                }
                return View();
        }
        // GET: Display employees in the user's department
        public async Task<IActionResult> AssignDeptRep(string searchString)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                MyUser rep = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int? deptheadDEPTID = rep.DeptId;
                Delegation deleg = await (from m in _context.Delegation.Where(p => p.DeptId == deptheadDEPTID) select m).FirstOrDefaultAsync();
                if (deleg != null && HttpContext.User.IsInRole("head"))
                {
                   
                    if (DateTime.Compare((DateTime)deleg.Startdate, DateTime.Now) < 0 && DateTime.Compare((DateTime)deleg.Enddate, DateTime.Now) > 0)
                    {
                        return Redirect("~/Departments/dutyoff");
                        //*yx: here needs a new page to show that head had already delegated his/her duty.
                    }
                }
                else if (deleg != null && HttpContext.User.IsInRole("actinghead"))
                {
                    if (DateTime.Compare((DateTime)deleg.Startdate, DateTime.Now) > 0 || DateTime.Compare((DateTime)deleg.Enddate, DateTime.Now) < 0)
                    {
                        rep.RoleId = 1;
                        _context.Update(rep);
                        ApplicationUser _user1 = await _userManager.FindByEmailAsync(rep.Email);
                        await _userManager.RemoveFromRoleAsync(_user1, "actinghead");
                        await _userManager.AddToRoleAsync(_user1, "employee");

                        return Redirect("~/Departments/expired");                       
                    }
                }
                int? id = rep.DeptId;
                if (id == null)
                {
                    return NotFound();
                }

                var department = await _context.Department.SingleOrDefaultAsync(m => m.DeptId == id);
                if (department == null)
                {
                    return NotFound();
                }
                ViewData["CurrentFilter"] = searchString;
                var employees = _context.MyUser.Where(u => u.DeptId == id && u.RoleId == 1);
                if (!String.IsNullOrEmpty(searchString))
                {
                    employees = employees.Where(e => e.Name.Contains(searchString));
                }
                List<MyUser> ulist = employees.ToList();
                ViewData["User"] = ulist;
                return View(department);
            }
            else return NotFound();
        }

        // POST: change department representative 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeptRep(string searchString, [Bind("DeptId,Name,DeptRep,Cpid")] Department department)
        {

            List<MyUser> alldepemployees = new List<MyUser>();
            alldepemployees = _context.MyUser.Where(x => x.DeptId == department.DeptId).ToList();
            MyUser myUser = new MyUser();
            myUser = _context.MyUser.Where(u => u.UserId == department.DeptRep).FirstOrDefault();

            #region Email
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SSIS", "team2adproject@gmail.com"));
            message.Subject = "New department rep";
            foreach (MyUser e in alldepemployees)
            {
                message.To.Add(new MailboxAddress(e.Name, e.Email));

                message.Body = new TextPart("plain")
                {
                    Text = "Dear " + e.Name + ", " +
                    "I am pleased to inform you all that " + myUser.Name + " is going to be our new Department Representative to be " +
                    "in charge of stationery collection for your department," +
                    "with effective from this coming Friday." +
                    Environment.NewLine +
                    "Regards" +
                    Environment.NewLine +
                    department.Name
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



            MyUser head = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
            int? id = head.DeptId;

            if (id != department.DeptId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    MyUser r = _context.MyUser.Where(em => em.RoleId == 7 && em.DeptId == id).ToList().FirstOrDefault();
                    if (r != null)
                    {
                        r.RoleId = 1;
                        _context.Update(r);
                        
                        ApplicationUser _user1 = await _userManager.FindByEmailAsync(r.Email);                       
                        await _userManager.RemoveFromRoleAsync(_user1, "representative");
                        await _userManager.AddToRoleAsync(_user1, "employee");
                    }
                    _context.Update(department);
                    await _context.SaveChangesAsync();
                    int? rid = await _context.Department.Where(d => d.DeptId == id).Select(d => d.DeptRep).SingleAsync();
                    MyUser repre = _context.MyUser.Find(rid);
                    repre.RoleId = 7;
                    ApplicationUser _user = await _userManager.FindByEmailAsync(repre.Email);                   
                    await _userManager.RemoveFromRoleAsync(_user, "employee");
                    await _userManager.AddToRoleAsync(_user, "representative");

                    _context.Update(repre);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DepartmentExists(department.DeptId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Redirect("~/Home/About");
            }
            ViewData["CurrentFilter"] = searchString;
            var employees = _context.MyUser.Where(u => u.DeptId == id & u.RoleId ==1);
            if (!String.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.Name.Contains(searchString));
            }
            return View(department);
        }

        private bool DepartmentExists(int id)
        {
            return _context.Department.Any(e => e.DeptId == id);
        }
    }
}
