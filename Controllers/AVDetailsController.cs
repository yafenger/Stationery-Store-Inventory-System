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
    public class AVDetailsController : Controller
    {
        private readonly SSISContext _context;

        public AVDetailsController(SSISContext context)
        {
            _context = context;
        }
     
        //Get:view details of the selected adjustment voucher.
        public async Task<IActionResult> ViewAV(int id)
        {
            if (HttpContext.User.IsInRole("clerk"))
            {
                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int userID = user.UserId;
                var avDetails = _context.Avdetails.Include(i => i.Item).Where(rd => rd.AdjustId == id);
                int TotalPrice = 0;
                List<AVDetails> avDetailsList = _context.Avdetails.Include(i => i.Item).Where(rd => rd.AdjustId == id).ToList();
                foreach (AVDetails aa in avDetailsList)
                {
                    int onePrice;
                    if (aa.Qtychanged == null || aa.Item.Unitprice == null)
                    {
                        onePrice = 0;
                    }
                    else
                    {
                        onePrice = (int)aa.Qtychanged * (int)aa.Item.Unitprice;
                    }
                    TotalPrice = TotalPrice + onePrice;
                }
                ViewData["TotalPrice"] = TotalPrice;
                int IDD = avDetailsList[0].AdjustId;
                ViewData["AdjsutID"] = IDD;
                return View(await avDetails.ToListAsync());
            }
            else return NotFound();
        }

        //POST :deal with the change for the current adjustment voucher details
        [HttpPost]
        public async Task<IActionResult> ViewAV(List<AVDetails> avDetails)
        {
            MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
            int userID = user.UserId;          
            int avdCount = avDetails.Count();
            for (int i = 0; i < avdCount; i++)
            {
                AVDetails existing = _context.Avdetails.Find(avDetails[i].AVDid);
                existing.Qtychanged = avDetails[i].Qtychanged;
                existing.Operations = avDetails[i].Operations;
                existing.Remarks = avDetails[i].Remarks;
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction("Index", "Adjustmentvouchers");
        }

        //POST:delete item in adjustment voucher details
        [HttpPost, ActionName("DeleteAVdetail")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteItem(int? detailsId)
        {
            if (detailsId == null)
            {
                return RedirectToAction(nameof(ViewAV));
            }
            AVDetails avDetail = await _context.Avdetails.SingleOrDefaultAsync(i => i.AVDid == detailsId);
            int avid = avDetail.AdjustId;
            _context.Avdetails.Remove(avDetail);
            await _context.SaveChangesAsync();
            var newlist = from m in _context.Avdetails.Where(m => m.AdjustId == avid) select m;
            List<Models.AVDetails> avdlist = await newlist.ToListAsync();
            return Redirect("~/AVDetails/ViewAv/"+avid);
        }


        private bool AVDetailsExists(int id)
        {
            return _context.Avdetails.Any(e => e.AVDid == id);
        }


    }
}
