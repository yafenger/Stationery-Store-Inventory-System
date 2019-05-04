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
    public class AdjustmentVouchersController : Controller
    {
        private readonly SSISContext _context;

        public AdjustmentVouchersController(SSISContext context)
        {
            _context = context;
        }

        //GET:show the view of AdjustmentVouchers
        public IActionResult Index()
        {
            if (HttpContext.User.IsInRole("clerk"))
            {
                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int userID = user.UserId;
                List<AdjustmentVoucher> originalList = _context.AdjustmentVoucher.Include(a => a.User).Where(b => b.UserId == userID).ToList();
                List<newAVModel> anotherList = new List<newAVModel>();
                foreach (AdjustmentVoucher abc in originalList)
                {
                    int TotalPrice = 0;
                    List<AVDetails> subList = _context.Avdetails.Include(x => x.Item).Where(a => a.AdjustId == abc.AdjustId).ToList();
                    for (int i = 0; i < subList.Count(); i++)
                    {
                        int onePrice = 0;
                        if (subList[i].Qtychanged == null)
                        {
                            subList[i].Qtychanged = 0;
                        }
                        onePrice = (int)subList[i].Qtychanged * (int)subList[i].Item.Unitprice;

                        TotalPrice = TotalPrice + onePrice;
                    }
                    newAVModel oaky = new newAVModel();
                    oaky.AdjustId = abc.AdjustId;
                    oaky.UserName = abc.User.Name;
                    oaky.Status = abc.Status;
                    oaky.Amount = TotalPrice;
                    anotherList.Add(oaky);
                }
                return View(anotherList.ToList());
            }
            else return NotFound();
        }

        // GET: AdjustmentVouchers/Main,add items for the chosen adjustment voucher.
        public async Task<IActionResult> Main(string searchString)
        {
            if (HttpContext.User.IsInRole("clerk"))
            {
                var item = from m in _context.Item
                           select m;

                if (!String.IsNullOrEmpty(searchString))
                {
                    item = item.Where(s => s.ItemName.Contains(searchString));
                }

                return View(await item.ToListAsync());
            }
            else return NotFound();
        }

        //POST:add items for the chosen adjustment voucher and turn to the details of adjustment voucher.
        [HttpPost]
        public async Task<IActionResult> Main(string itemid, int? id)
        {
            MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
            int userID = user.UserId;
            AdjustmentVoucher AV;
            if (id == 0)
            {
                AV = new AdjustmentVoucher
                {
                    UserId = userID,
                    Status = "Pending"
                };
                _context.Add(AV);
                await _context.SaveChangesAsync();
                id = AV.AdjustId;
            }
            else
            {
                AV = _context.AdjustmentVoucher.Find(id);
            }
            AVDetails checkAVD = _context.Avdetails.Where(d => d.AdjustId == AV.AdjustId & d.ItemId == itemid).SingleOrDefault();
            AVDetails avd;
            if (checkAVD == null)
            {
                avd = new AVDetails();
                avd.AdjustId = AV.AdjustId;
                avd.ItemId = itemid;
                _context.Add(avd);
                await _context.SaveChangesAsync();
            }
            return Redirect("~/AVDetails/ViewAv/" + id);
        }


        //GET: Pending,for manager or supervisor to view the pending requests.
        public async Task<IActionResult> Pending()
        {
            if (HttpContext.User.IsInRole("manager") || HttpContext.User.IsInRole("supervisor"))
            {
                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).Include(p => p.Dept).ToList().FirstOrDefault();
                int? deptheadDEPTID = user.DeptId;
                var sSISContext = _context.AdjustmentVoucher.Include(r => r.User).Where(d => d.User.DeptId == deptheadDEPTID && d.Status != "Pending");
                var sSISContext1 = _context.Avdetails.Include(r => r.Item);
                List<AdjustmentVoucher> avlist = await sSISContext.ToListAsync();
                List<newAVModel> managerList = new List<newAVModel>();
                List<newAVModel> supervisorList = new List<newAVModel>();
                List<AVDetails> avdlist = await sSISContext1.ToListAsync();
                foreach (AdjustmentVoucher a in avlist)
                {
                    int total = 0;
                    foreach (AVDetails avd in avdlist)
                    {
                        if (avd.AdjustId == a.AdjustId)
                        {
                            if (avd.Qtychanged == null) avd.Qtychanged = 0;
                            total += (int)avd.Item.Unitprice * (int)avd.Qtychanged;
                        }
                    }
                    if (total > 250)
                    {
                        newAVModel nw = new newAVModel();
                        nw.AdjustId = a.AdjustId;
                        nw.Amount = total;
                        nw.Status = a.Status;
                        nw.UserName = a.User.Name;
                        managerList.Add(nw);
                    }
                    else if (total < 250)
                    {
                        newAVModel nw = new newAVModel();
                        nw.AdjustId = a.AdjustId;
                        nw.Amount = total;
                        nw.Status = a.Status;
                        nw.UserName = a.User.Name;
                        supervisorList.Add(nw);
                    }
                    //Assume that 250 is the absolute changing number,not the total earn or lost.
                }
                if (HttpContext.User.IsInRole("manager"))
                {
                    return View(managerList);
                }
                else if (HttpContext.User.IsInRole("supervisor"))
                {
                    return View(supervisorList);
                }
                return View(await sSISContext.ToListAsync());
            }
            else return NotFound();
        }

        // GET: Requests/Action/id,for manager or supervisor to view the details of the chosen adjustment voucher.
        public async Task<IActionResult> Action(int? id)
        {
            if (HttpContext.User.IsInRole("manager") || HttpContext.User.IsInRole("supervisor"))
            {
                bool flag = true;
                if (id == null)
                {
                    return NotFound();
                }
                var av = await _context.AdjustmentVoucher.Include(u => u.User).Include(u => u.Avdetails).ThenInclude(rd => rd.Item).SingleOrDefaultAsync(m => m.AdjustId == id);
                if (av == null)
                {
                    return NotFound();
                }

                List<AVDetails> avdlist = new List<AVDetails>();

                foreach (var rd in av.Avdetails)
                {
                    avdlist.Add(rd);
                }
                ViewData["avdlist"] = avdlist;

                int totalAmt = 0;
                foreach (AVDetails avd in avdlist)
                {
                    int thisAmt = (int)avd.Item.Unitprice * (int)avd.Qtychanged;
                    totalAmt += thisAmt;
                    if (avd.Operations == "-")
                    {
                        if (avd.Qtychanged > _context.Item.Where(p => p.ItemId == avd.ItemId).FirstOrDefault().Stock) flag = false; 
                    }
                }
                ViewData["StockCheck"] = flag;
                ViewData["totalAmt"] = totalAmt;
                return View(av);
            }
            else return NotFound();
        }

        // POST: Requests/Action/id,deal with the status of adjustment voucher according to operation.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Action(string SubmitButton, int id, [Bind("AdjustId,UserId")] AdjustmentVoucher adjustmentVoucher)
        {
           
            #region Email to remind adjustment voucher approved/rejected
            if (adjustmentVoucher != null)
            {
                var message = new MimeMessage();
                MyUser e = _context.MyUser.Where(p => p.UserId == adjustmentVoucher.UserId).FirstOrDefault();
                message.From.Add(new MailboxAddress("Department Head", "team2adproject@gmail.com"));
                message.To.Add(new MailboxAddress(e.Name, e.Email));
                message.Subject = "Your Request is handled";
                message.Body = new TextPart("plain")
                {
                    Text = "Dear staff, " +
                    "" +
                    "Your order request has been processed by your department head. You may login to SSIS system to check for results." +
                    "" +
                    "This is an automatic-generated email. Please do not reply." +
                    "" +
                    "Sincerely," +
                    "Department Head"
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

            if (id != adjustmentVoucher.AdjustId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    List<AVDetails> rdlist = _context.Avdetails.Where(m => m.AdjustId == adjustmentVoucher.AdjustId).ToList();
                    string buttonClicked = SubmitButton;
                    if (buttonClicked == "Approved")
                    {
                        adjustmentVoucher.Status = "Approved";
                        _context.Update(adjustmentVoucher);
                        await _context.SaveChangesAsync();
                        foreach(AVDetails avd in rdlist)
                        {
                            Item thisItem = _context.Item.Find(avd.ItemId);
                            if (avd.Operations == "-")
                            {                            
                                thisItem.Stock -= avd.Qtychanged;
                                _context.Update(thisItem);
                                await _context.SaveChangesAsync();
                            }
                            else if (avd.Operations == "+")
                            {
                                thisItem.Stock += avd.Qtychanged;
                                _context.Update(thisItem);
                                await _context.SaveChangesAsync();
                            }                      
                        }
                    }
                    else if (buttonClicked == "Rejected")
                    {
                        adjustmentVoucher.Status = "Rejected";
                        _context.Update(adjustmentVoucher);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdjustmentVoucherExists(adjustmentVoucher.AdjustId))
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
            return View(adjustmentVoucher);
        }

        //POST:delete the whole Voucher not submitted yet.
        [HttpPost, ActionName("DeleteVoucher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int? VoucherId)
        {
            if (VoucherId == null)
            {
                return RedirectToAction(nameof(Index));
            }
            AdjustmentVoucher voucher = await _context.AdjustmentVoucher.SingleOrDefaultAsync(i => i.AdjustId == VoucherId);
            List<AVDetails> voucherDetailsList = _context.Avdetails.Where(x => x.AdjustId == VoucherId).ToList();
            foreach (AVDetails item in voucherDetailsList)
            {
                AVDetails current = _context.Avdetails.Find(item.AVDid);
                _context.Avdetails.Remove(current);
                await _context.SaveChangesAsync();
            }
            _context.AdjustmentVoucher.Remove(voucher);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Post :submit the adjustment voucher.
        [HttpPost]
        public async Task<IActionResult> SubmitAV(int? VoucherId)
        {
            MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
            int userID = user.UserId;

            AdjustmentVoucher thisAV = _context.AdjustmentVoucher.Find(VoucherId);
            thisAV.Status = "Submitted";
            _context.Update(thisAV);
            await _context.SaveChangesAsync();

            #region Email to supervisor or manager to remind a new adjustment voucher
            MyUser supervisor = new MyUser();
            supervisor = _context.MyUser.Where(x => x.RoleId == 3).FirstOrDefault();
            MyUser manager = new MyUser();
            manager = _context.MyUser.Where(x => x.RoleId == 4).FirstOrDefault();
            List<AVDetails> avlist = new List<AVDetails>();
            avlist = _context.Avdetails.Where(x => x.AdjustId == VoucherId).Include(p => p.Item).ToList();
            int? total = 0;
            foreach (AVDetails av in avlist)
            {
                total += av.Item.Unitprice * av.Qtychanged;
            }
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("SSIS", "team2adproject@gmail.com"));
            //to manager (>250)
            if (total > 250)
            {
                message.To.Add(new MailboxAddress(manager.Name, manager.Email));
                message.Subject = "New Adjustment Voucher for your approval";
                message.Body = new TextPart("plain")
                {
                    Text = "Dear  " + manager.Name +
                    "There is an adjustment voucher of " + "$" + total + " which require your approval. Please login to SSIS system to approve it." +
                    "" +
                    "This is an automatic-generated email. Please do not reply."
                };
            }
            //to supervisor (<=250)
            else
            {
                message.To.Add(new MailboxAddress(supervisor.Name, supervisor.Email));
                message.Subject = "New Adjustment Voucher for your approval";
                message.Body = new TextPart("plain")
                {
                    Text = "Dear  " + supervisor.Name +
                    "There is an adjustment voucher of " + "$" + total + " which require your approval. Please login to SSIS system to approve it." +
                    "" +
                    "This is an automatic-generated email. Please do not reply."
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

            return RedirectToAction("Index", "Adjustmentvouchers");
        }

        private bool AdjustmentVoucherExists(int id)
        {
            return _context.AdjustmentVoucher.Any(e => e.AdjustId == id);
        }

    }
}
