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
    public class RequestsController : Controller
    {
        private readonly SSISContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public RequestsController(SSISContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        //GET: Get the pending requests within the department.
        public async Task<IActionResult> Pending()
        {
            if (HttpContext.User.Identity.IsAuthenticated && (HttpContext.User.IsInRole("head") || HttpContext.User.IsInRole("actinghead")))
            {
                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).Include(p => p.Dept).ToList().FirstOrDefault();
                int deptheadDEPTID = (int)user.DeptId;
                Delegation deleg = await (from m in _context.Delegation.Where(p => p.DeptId == deptheadDEPTID) select m).FirstOrDefaultAsync();
                if (deleg!= null&& HttpContext.User.IsInRole("head"))
                {
                    if(DateTime.Compare((DateTime)user.Dept.Delegation.FirstOrDefault().Startdate,DateTime.Now)<0&& DateTime.Compare((DateTime)user.Dept.Delegation.FirstOrDefault().Enddate, DateTime.Now) > 0)
                    {

                        return Redirect("~/Departments/dutyoff");
                    }
                }
                else if(user.Dept.Delegation.FirstOrDefault() != null && HttpContext.User.IsInRole("actinghead"))
                {
                    if (DateTime.Compare((DateTime)user.Dept.Delegation.FirstOrDefault().Startdate, DateTime.Now) > 0 || DateTime.Compare((DateTime)user.Dept.Delegation.FirstOrDefault().Enddate, DateTime.Now) < 0)
                    {
                        user.RoleId = 1;
                        _context.Update(user);
                        ApplicationUser _user1 = await _userManager.FindByEmailAsync(user.Email);
                        await _userManager.RemoveFromRoleAsync(_user1, "actinghead");
                        await _userManager.AddToRoleAsync(_user1, "employee");

                        return Redirect("~/Departments/expired");
                    }
                }
                var sSISContext = _context.Request.Include(r => r.User).Where(d => d.User.DeptId == deptheadDEPTID & d.Status != "Pending");
                return View(await sSISContext.ToListAsync());
            }
            else return NotFound();
        }

        private bool RequestExists(int id)
        {
            return _context.Request.Any(e => e.RequestId == id);
        }

        //View the requests within the department.
        public async Task<IActionResult> viewRequests(string searchString)
        {
            if (HttpContext.User.Identity.IsAuthenticated && (HttpContext.User.IsInRole("employee") || HttpContext.User.IsInRole("representative") || HttpContext.User.IsInRole("head") || HttpContext.User.IsInRole("actinghead") || HttpContext.User.IsInRole("clerk")))
            {
                MyUser e = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).Include(p => p.Dept).ToList().FirstOrDefault();
                int? eDEPTID = e.DeptId;

                var requests = _context.Request.Include(d => d.User).Where(u => u.User.DeptId == eDEPTID);

                ViewData["CurrentFilter"] = searchString;
                if (!String.IsNullOrEmpty(searchString))
                {
                    requests = requests.Where(s => s.User.Name.Contains(searchString));
                }

                List<Request> rlist = requests.ToList();
                ViewData["requests"] = rlist;
                return View(await requests.ToListAsync());
            }
            return NotFound();
        }

        // GET: View the details of the selected requests.
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.User.IsInRole("employee") || HttpContext.User.IsInRole("representative") || HttpContext.User.IsInRole("head") || HttpContext.User.IsInRole("actinghead") || HttpContext.User.IsInRole("clerk"))
            {
                List<RequestDetails> details = null;
                if (id == null)
                {
                    return NotFound();
                }

                var request = await _context.Request
                    .Include(d => d.RequestDetails)
                    .SingleOrDefaultAsync(m => m.RequestId == id);
                if (request == null)
                {
                    return NotFound();
                }
                else
                {
                    details = await _context.RequestDetails.Include(d => d.Item).Where(d => d.RequestId == id).ToListAsync();
                    ViewData["details"] = details;
                }
                return View();
            }
            else return NotFound();
        }

        // GET: head or head view the details the selected request.
        public async Task<IActionResult> Action(int? id)
        {
            if (HttpContext.User.IsInRole("head") || HttpContext.User.IsInRole("actinghead"))
            {
                if (id == null)
                {
                    return NotFound();
                }
                var request = await _context.Request.Include(u => u.User).Include(u => u.RequestDetails).ThenInclude(rd => rd.Item).SingleOrDefaultAsync(m => m.RequestId == id);
                if (request == null)
                {
                    return NotFound();
                }

                List<RequestDetails> rdlist = new List<RequestDetails>();

                foreach (var rd in request.RequestDetails)
                {
                    rdlist.Add(rd);
                }
                ViewData["rdlist"] = rdlist;
                return View(request);
            }
            return NotFound();
        }

        // POST: deal with the operation to the request.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Action(string SubmitButton, int id, [Bind("RequestId,UserId,Remarks,IsCompleted")] Request request)
        {


            MyUser employee = new MyUser();
            employee = _context.MyUser.Where(x => x.UserId == request.UserId).ToList().FirstOrDefault();

            #region Email to remind the requester.
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SSIS", "team2adproject@gmail.com"));
            message.To.Add(new MailboxAddress(employee.Name, employee.Email));
            message.Subject = "Your Request is handled";
            message.Body = new TextPart("plain")
            {
                Text = "Dear " + employee.Name + ", " +
                "" +
                "Your order request has been processed by your department head. You may login to SSIS system to check for results." +
                "" +
                "This is an automatic-generated email. Please do not reply."
            };
            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("team2adproject@gmail.com", "team2team2");
                client.Send(message);
                client.Disconnect(true);
            }
            #endregion

            if (id != request.RequestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    List<RequestDetails> rdlist = _context.RequestDetails.Where(m => m.RequestId == request.RequestId).ToList();
                    string buttonClicked = SubmitButton;
                    if (buttonClicked == "Approved")
                    {
                        request.Status = "Approved";
                        request.Approvaltime = DateTime.Now;

                        for (int i = 0; i < rdlist.Count; i++)
                        {
                            string rdlistitemid = rdlist[i].ItemId;
                            int rdlistitemstock = rdlist[i].RequestedQty;
                            Item item = await _context.Item.SingleOrDefaultAsync(m => m.ItemId == rdlistitemid);
                            int itemStock = (int)item.Stock;

                            if (item.Stock <= 0)
                            {
                                rdlist[i].Type = "Preorder";
                                _context.Update(rdlist[i]);
                                await _context.SaveChangesAsync();
                            }
                            else if (rdlistitemstock <= itemStock)
                            {
                                rdlist[i].Type = "Order";
                                _context.Update(rdlist[i]);
                                item.Stock = itemStock - rdlistitemstock;
                                _context.Update(item);
                                await _context.SaveChangesAsync();
                            }

                            else
                            {
                                int preorderqty = rdlist[i].RequestedQty - itemStock;
                                rdlist[i].RequestedQty = itemStock;
                                rdlist[i].Type = "Order";
                                _context.Update(rdlist[i]);

                                RequestDetails rdnew = new RequestDetails();
                                rdnew.RequestId = rdlist[i].RequestId;
                                rdnew.ItemId = rdlist[i].ItemId;
                                rdnew.RequestedQty = preorderqty;
                                rdnew.Type = "Preorder";
                                _context.Add(rdnew);
                                item.Stock = 0;
                                _context.Update(item);
                                await _context.SaveChangesAsync();
                            }
                        }
                        _context.Update(request);
                        await _context.SaveChangesAsync();
                    }
                    else if (buttonClicked == "Rejected")
                    {
                        request.Status = "Rejected";
                        request.Approvaltime = DateTime.Now;
                        _context.Update(request);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RequestExists(request.RequestId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Pending));
            }
            ViewData["UserId"] = new SelectList(_context.MyUser, "UserId", "Email", request.UserId);
            return View(request);
        }

        //GET: View the item lists.
        public async Task<IActionResult> Cart(string searchString)
        {
            if (HttpContext.User.Identity.IsAuthenticated && (HttpContext.User.IsInRole("employee") || HttpContext.User.IsInRole("representative") || HttpContext.User.IsInRole("clerk")))
            {
                var items = _context.Item.Include(i => i.Cat).Where(e => e.Stock >= 0);
                List<Item> ilist = new List<Item>();
                ViewData["CurrentFilter"] = searchString;
                if (!String.IsNullOrEmpty(searchString))
                {
                    items = items.Where(e => e.ItemName.Contains(searchString));
                }
                return View(await items.ToListAsync());
            }
            return NotFound();
        }

        //POST: add item and jump to request details view.
        [HttpPost]
        public async Task<IActionResult> Cart(string itemid, int? id)
        {
            MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
            int userID = user.UserId;

            Request r = _context.Request.Include(d => d.User).Where(d => d.User.UserId == userID && (d.Status == "Pending" || d.Status == "Submitted")).ToList().FirstOrDefault();
            Request req;
            if (r == null)
            {
                req = new Request();
                req.UserId = userID;
                req.Status = "Pending";
                _context.Add(req);
                await _context.SaveChangesAsync();
            }
            else
            {
                req = r;
                req.Status = "Pending";
                _context.Update(req);
                await _context.SaveChangesAsync();
            }
            RequestDetails checkRD = _context.RequestDetails.Where(d => d.RequestId == req.RequestId & d.ItemId == itemid).SingleOrDefault();
            RequestDetails rd;
            if (checkRD == null)
            {
                rd = new RequestDetails();
                rd.RequestId = req.RequestId;
                rd.ItemId = itemid;
                rd.RequestedQty = 1;
                _context.Add(rd);
                await _context.SaveChangesAsync();
            }
            else
            {
                rd = checkRD;
                rd.RequestedQty += 1;
                _context.Update(rd);
                await _context.SaveChangesAsync();
            }
            return Redirect("~/Requestdetails/raisedReqDetails");
        }
    }
}

