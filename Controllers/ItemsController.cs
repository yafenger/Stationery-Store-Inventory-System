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
    public class ItemsController : Controller
    {
        private readonly SSISContext _context;

        public ItemsController(SSISContext context)
        {
            _context = context;
        }

        // GET: show the view of the items.
        public async Task<IActionResult> Index(string searchString,string id)
        {
            if (HttpContext.User.Identity.IsAuthenticated && (HttpContext.User.IsInRole("clerk")|| HttpContext.User.IsInRole("supervisor")))
            {
                var item = from m in _context.Item select m;
                if (!String.IsNullOrEmpty(searchString))
                {
                    item = item.Where(s => s.ItemName.Contains(searchString));
                }

                //* yx:set the submit to a get method
                if (id == "preorder")
                {
                    var pre = from m in _context.RequestDetails.Where(p => p.Type == "preorder").Include(p => p.Item) select m;
                    List<Item> ilist = await item.ToListAsync();
                    List<RequestDetails> rdlist = await pre.ToListAsync();
                    foreach (Item i in ilist)
                    {
                        foreach (RequestDetails rds in rdlist)
                        {
                            if (i.ItemId == rds.ItemId && i.Stock >= rds.RequestedQty)
                            {                                
                                try
                                {
                                    rds.Type = "Order";
                                    _context.Update(rds);

                                    i.Stock -= rds.RequestedQty;
                                    _context.Update(i);
                                    await _context.SaveChangesAsync();
                                    break;
                                }
                                catch (DbUpdateConcurrencyException)
                                {
                                    throw;
                                }
                            }
                        }
                    }
                    item = from m in _context.Item select m;
                }
                return View(await item.ToListAsync());
            }
            else return NotFound();
        }

        //View of create new item
        public IActionResult Create()
        {
            if (HttpContext.User.IsInRole("clerk"))
            {
                ViewData["CatId"] = new SelectList(_context.Category, "CatId", "CatName");
                return View();
            }
            else return NotFound();
        }

        // POST: create new item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ItemId,ItemName,CatId,ReorderLvl,ReorderQty,Uom,Stock,Unitprice")] Item item)
        {
            if (ModelState.IsValid)
            {
                _context.Add(item);
                await _context.SaveChangesAsync();

                #region Email to remind departments of new item.
                List<MyUser> employees = new List<MyUser>();
                employees = _context.MyUser.Where(x => x.RoleId == 1).ToList();

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("SSIS", "team2adproject@gmail.com"));
                foreach (MyUser e in employees)
                {
                    message.To.Add(new MailboxAddress(e.Name, e.Email));
                }
                message.Subject = item.ItemName + " now available!";
                message.Body = new TextPart("plain")
                {
                    Text = "Dear colleagues, " +
                    "Please be informed that a new item called " + item.ItemName +
                    " is now available in the stationary catalogue. You can now order it." +
                    "This is an automatically generated email. Please do not reply."
                };
                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("team2adproject@gmail.com", "team2team2");
                    client.Send(message);
                    client.Disconnect(true);
                }
                #endregion 

                return RedirectToAction(nameof(Index));
            }
            ViewData["CatId"] = new SelectList(_context.Category, "CatId", "CatName", item.CatId);
            return View(item);
        }

        private bool ItemExists(string id)
        {
            return _context.Item.Any(e => e.ItemId == id);
        }


        // GET: view the reorder level and reorder quantity of items
        public async Task<IActionResult> ManageReorder(string searchString)
        {
            var item = from m in _context.Item select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                item = item.Where(s => s.ItemName.Contains(searchString));
            }
            @ViewData["currentFilter"] = searchString;
            return View(await item.ToListAsync());
        }


        // POST: deal with the reorder level and reorder quantity of items
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageReorder(List<Item> items)
        {
            int itemcount = items.Count();
            for (int i = 0; i < itemcount; i++)
            {
                Item existing = _context.Item.Find(items[i].ItemId);
                existing.ReorderLvl = items[i].ReorderLvl;
                existing.ReorderQty = items[i].ReorderQty;
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: get the view of items and show the stock
        public async Task<IActionResult> ManageStocks(string searchString)
        {
            var item = from m in _context.Item select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                item = item.Where(s => s.ItemName.Contains(searchString));
            }
            @ViewData["currentFilter"] = searchString;
            return View(await item.ToListAsync());
        }


        // POST: deal with the stock of items
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageStocks(List<Item> items)
        {
            var requests = from m in _context.RequestDetails.Include(p => p.Item).Where(p => p.Type != "Order") select m;
            List<RequestDetails> rdlist = await requests.ToListAsync();
            int itemcount = items.Count();
            for (int i = 0; i < itemcount; i++)
            {
                Item existing = _context.Item.Find(items[i].ItemId);
                existing.Stock = items[i].Stock;
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }
            List<Item> ilist = await (from m in _context.Item.Where(p => p.Stock > 0) select m).ToListAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
