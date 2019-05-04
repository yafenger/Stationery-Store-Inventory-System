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

namespace ADProjectBase2.Controllers
{
    public class RequestDetailsController : Controller
    {
        private readonly SSISContext _context;

        public RequestDetailsController(SSISContext context)
        {
            _context = context;
        }
   

        private bool RequestDetailsExists(int id)
        {
            return _context.RequestDetails.Any(e => e.ReqDetailsId == id);
        }

        //GET:shopping cart page for the request details
        public async Task<IActionResult> raisedReqDetails()
        {
            if (HttpContext.User.IsInRole("clerk") || HttpContext.User.IsInRole("employee") || HttpContext.User.IsInRole("representative"))
            {
                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).Include(p => p.Dept).ToList().FirstOrDefault();
                int id = user.UserId;

                int rId = _context.Request.Where(r => r.UserId == id & (r.Status == "Pending" || r.Status == "Submitted")).Select(r => r.RequestId).FirstOrDefault();
                var requestDetails = _context.RequestDetails.Include(i => i.Item)
                    .Where(rd => rd.RequestId == rId);
                return View(await requestDetails.ToListAsync());
            }
            else return NotFound();
        }
        //POST: shopping cart post for the request details
        [HttpPost]
        public async Task<IActionResult> raisedReqDetails(List<RequestDetails> requestDetails)
        {
            MyUser u = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).Include(p => p.Dept).ToList().FirstOrDefault();
            if (requestDetails != null)
            {
                int j = 0;
                foreach (RequestDetails rd in requestDetails)
                {
                    if ((rd.RequestedQty > 0) && (rd.RequestedQty < 1000))
                    {
                        j = j + 1;
                    }
                }
                if (requestDetails.Count() == j)
                {
                    {
                        Request thisRequest = _context.Request.Where(x => x.RequestId == requestDetails[0].RequestId).FirstOrDefault();
                        thisRequest.Status = "Submitted";
                        _context.Update(thisRequest);
                        await _context.SaveChangesAsync();

                        int requestDetailsCount = requestDetails.Count();
                        for (int i = 0; i < requestDetailsCount; i++)
                        {
                            RequestDetails existing = _context.RequestDetails.Find(requestDetails[i].ReqDetailsId);
                            existing.RequestedQty = requestDetails[i].RequestedQty;
                            _context.Update(existing);
                            await _context.SaveChangesAsync();
                        }

                        #region Email
                        bool flag = true;
                        MyUser headofdepartment = new MyUser();
                        Department department = await (from m in _context.Department.Where(p => p.DeptId == u.DeptId) select m).FirstOrDefaultAsync();
                        Delegation deleg = await (from m in _context.Delegation.Where(p => p.DeptId == u.DeptId) select m).FirstOrDefaultAsync();
                        if (deleg != null)
                        {
                            if ((DateTime.Compare((DateTime)deleg.Startdate, DateTime.Now) > 0) || DateTime.Compare((DateTime)deleg.Enddate, DateTime.Now) < 0)
                                flag = false;
                        }
                        headofdepartment = _context.MyUser.Where(x => x.RoleId == 6 && x.DeptId == department.DeptId).ToList().FirstOrDefault();
                        if(headofdepartment==null||flag==false) headofdepartment= _context.MyUser.Where(x => x.RoleId == 2 && x.DeptId == department.DeptId).ToList().FirstOrDefault();
                        if (headofdepartment != null)
                        {
                            var message = new MimeMessage();
                            message.From.Add(new MailboxAddress("SSIS", "team2adproject@gmail.com"));
                            
                            message.To.Add(new MailboxAddress(headofdepartment.Name, headofdepartment.Email));
                            message.Subject = "New order request";
                            message.Body = new TextPart("plain")
                            {
                                Text = "Dear " + headofdepartment.Name + ", " +
                                "There is a new order request by your employee. " +
                                "Please login to SSIS system for further action. " +
                                Environment.NewLine +
                                Environment.NewLine +
                                "This is an auto-generated email. Please do not reply."
                            };
                            using (var client = new SmtpClient())
                            {
                                client.Connect("smtp.gmail.com", 587, false);
                                client.Authenticate("team2adproject@gmail.com", "team2team2");
                                client.Send(message);
                                client.Disconnect(true);
                            }
                        }
                        #endregion

                    }
                    return RedirectToAction("viewRequests", "Requests");
                }
                else
                {
                    return RedirectToAction("raisedReqDetails");
                }

            }
            else return RedirectToAction("viewRequests", "Requests");           
        }

        //delete item in request details
        [HttpPost, ActionName("DeleteItem")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int? detailsId)
        {
            if (detailsId == null)
            {
                return RedirectToAction(nameof(raisedReqDetails));
            }
            var requestDetail = await _context.RequestDetails.SingleOrDefaultAsync(i => i.ReqDetailsId == detailsId);
            _context.RequestDetails.Remove(requestDetail);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(raisedReqDetails));
        }
    }
}
