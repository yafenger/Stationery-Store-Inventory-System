using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADProjectBase2.Models;
using Microsoft.AspNetCore.Http;
using MimeKit;
using MailKit.Net.Smtp;

namespace ADProjectBase2.Controllers
{
    public class PODetailsController : Controller
    {
        private readonly SSISContext _context;
        static List<string> idlist = new List<string>();
        static int supplierId = 5;
        static bool IsChanged = false;
        static bool IsPosted = false;
        static List<string> snamelist = new List<string>();      
        static List<PurchaseOrder> polist;

        public PODetailsController(SSISContext context)        
        {
            _context = context;
        }

        //GET: View order history to suppliers
        public async Task<IActionResult> ViewOrderHistory(int sy,int ey,string sm,string em, int? id)
        {
            if (HttpContext.User.Identity.IsAuthenticated&&(HttpContext.User.IsInRole("clerk")|| HttpContext.User.IsInRole("manager")|| HttpContext.User.IsInRole("supervisor")))
            {
                List<String> MonthList = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                                                            "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"};
                List<int> YearList = new List<int>();
                
                var po = from m in _context.PurchaseOrder.Include(p => p.Podetails).Include(p=>p.Supplier) select m;
                int latestYear = DateTime.Now.Year;
                if (po.LastOrDefault() != null) latestYear =  po.LastOrDefault().PurchaseDate.Year;

                for(int i = 0; i <= 2; i++)
                {
                    YearList.Add(latestYear-2+i);
                }
                var pod = from m in _context.Podetails.Include(p => p.Item) select m;
                if(sy!=0&&ey!=0&& !string.IsNullOrEmpty(sm)&&!string.IsNullOrEmpty(em))
                {
                    id = 0;
                    DateTime startTime = new DateTime(sy,MonthList.IndexOf(sm)+1,1);
                    DateTime endTime = startTime.AddMonths(1).AddDays((-startTime.Day));
                    po = po.Where(p => DateTime.Compare((DateTime)p.PurchaseDate, startTime) > 0 && DateTime.Compare((DateTime)p.PurchaseDate, endTime) < 0);
                    polist = await po.ToListAsync();
                }

                if (id == null||id==0)
                {
                    polist = await po.ToListAsync();
                }
                List<PODetails> podlist = await pod.ToListAsync();
                List<PODetails> podmodel = new List<PODetails>();

                List<double> plist = new List<double>();
                foreach(PurchaseOrder p in polist)
                {
                    double total = 0;
                    foreach(PODetails pd in podlist)
                    {
                        if (pd.PoId == p.PoId)
                        {
                            if (pd.QtyOrdered == null) pd.QtyOrdered = 0;
                            total += (int)pd.QtyOrdered * (double)pd.Item.Unitprice;
                        }
                    }
                    plist.Add(total);
                }
                if (id!=0&&id!=null)
                {
                    double price = 0;
                    foreach(PurchaseOrder po1 in polist)
                    {
                        if (po1.PoId == id)
                        {
                            price = plist[polist.IndexOf(po1)];
                        }
                    }
                    podmodel = await (from m in _context.Podetails.Where(p => p.PoId == id) select m).ToListAsync();
                    polist = await (from m in _context.PurchaseOrder.Where(p => p.PoId == id).Include(p=>p.Supplier) select m).ToListAsync();
                    plist = new List<double> { price };
                }
                var pohistoryModel = new POHistoryModel
                {
                    pricelist = plist,
                    POList = polist,
                    podList = podmodel,
                    startMonth = new SelectList(MonthList),
                    endMonth = new SelectList(MonthList),
                    startYear = new SelectList(YearList),
                    endYear = new SelectList(YearList)
                };
                return View(pohistoryModel);
            }
            return NotFound();
        }

        // GET: Get the view of the order list within chosen supplier.
        public async Task<IActionResult> Index(string searchString, [FromQuery(Name = "btn")] string button, string id, [FromQuery(Name = "sup")] string sup)
        {
            if (HttpContext.User.Identity.IsAuthenticated && (HttpContext.User.IsInRole("clerk") || HttpContext.User.IsInRole("manager") || HttpContext.User.IsInRole("supervisor")))
            {
                #region logic code
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    string s = HttpContext.User.Identity.ToString();
                }

                if (snamelist.Count == 0)
                {
                    IQueryable<string> genreQuery = from m in _context.Supplier select m.SupplierName;
                    snamelist = await genreQuery.Distinct().ToListAsync();
                }
                var userContext = from m in _context.MyUser select m;
                List<MyUser> mlist = await userContext.ToListAsync();
                if (!string.IsNullOrEmpty(sup))
                {
                    var supcontext1 = from m in _context.Supplier where m.SupplierName == sup select m;
                    Supplier su1 = supcontext1.ToList().First();
                    if (su1.SupplierId != supplierId)
                    {
                        supplierId = su1.SupplierId;
                        IsChanged = true;
                    }

                    var supcontext = from m in _context.Supplier where m.SupplierName == sup select m;
                    Supplier su = supcontext.ToList().First();
                    if (snamelist[0] != su.SupplierName)
                    {
                        snamelist[snamelist.IndexOf(su.SupplierName)] = snamelist[0];
                        snamelist[0] = su.SupplierName;
                    }
                }
                var sSISContext = from m in _context.Podetails.Include(p => p.Item).Include(p => p.Po) select m;
                var itemContext = from m in _context.Item.Include(p => p.Cat) where m.Cat.Supplier1 == supplierId || m.Cat.Supplier2 == supplierId || m.Cat.Supplier3 == supplierId select m;
                var rds = from m in _context.RequestDetails.Include(p => p.Request) where m.Type == "Preorder" select m;
                var cats = from m in _context.Category.Include(p => p.Supplier1Navigation).Include(p => p.Supplier2Navigation).Include(p => p.Supplier3Navigation).Include(p => p.Item) select m;

                List<int> numlist = new List<int>();
                List<int> reoderlist = new List<int>();
                List<PODetails> podlist = new List<PODetails>();
                List<RequestDetails> rdlist = await rds.ToListAsync();

                if (!IsChanged)
                {
                    if (!IsPosted)
                    {
                        if (!string.IsNullOrEmpty(id))
                        {
                            if (idlist.Count > 0)
                            {
                                if (idlist.IndexOf(id) == -1) idlist.Add(id);
                            }
                            else idlist.Add(id);
                        }
                        if (idlist.Count > 0)
                        {
                            foreach (string s in idlist)
                            {
                                PODetails pods = new PODetails();
                                int qty = 0;
                                pods.ItemId = s;
                                pods.Item = itemContext.Where(p => p.ItemId == s).ToList().First();
                                if (pods.Item.Stock < pods.Item.ReorderLvl) qty += (int)pods.Item.ReorderQty;
                                List<string> suplist = new List<string>();
                                podlist.Add(pods);
                            }
                        }
                    }
                    else IsPosted = false;
                }
                else
                {
                    idlist = new List<string>();
                    IsChanged = false;
                }

                if (!String.IsNullOrEmpty(searchString))
                {
                    itemContext = itemContext.Where(a => a.ItemName.Contains(searchString));
                }
                if (podlist.Count != 0)
                {
                    foreach (PODetails pod in podlist)
                    {
                        int total = 0;
                        foreach (RequestDetails rd in rdlist)
                        {
                            if (rd.ItemId == pod.ItemId) total += rd.RequestedQty;
                        }
                        numlist.Add(total);
                        if (pod.Item.ReorderLvl != null && pod.Item.ReorderQty != null)
                        {
                            reoderlist.Add(total + (int)pod.Item.Stock < (int)pod.Item.ReorderLvl ? (int)pod.Item.ReorderQty : 0);
                        }
                        else reoderlist.Add(0);

                    }
                }
                List<Item> ilist = await itemContext.ToListAsync();
                var podVM = new RaiseOrderViewModel
                {
                    pods = podlist,
                    podetails = podlist,
                    items = ilist,
                    preorderNums = numlist,
                    orderNums = reoderlist,
                    supplierList = new SelectList(snamelist),
                    Suplist = sup,
                    SearchString = searchString
                };
                return View(podVM);
                #endregion
            }
            return NotFound();
        }

        //POST: submit the order to supplier.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(List<int> numlist)
        {            
            PurchaseOrder po = new PurchaseOrder();
            var itemContext = from m in _context.Item.Include(p => p.Cat) where m.Cat.Supplier1 == supplierId || m.Cat.Supplier2 == supplierId || m.Cat.Supplier3 == supplierId select m;
            try
            {
                po.SupplierId = supplierId;
                po.PurchaseDate = DateTime.Now.Date;
                _context.Add(po);
                await _context.SaveChangesAsync();
                var pocontext = from m in _context.PurchaseOrder select m;
                List<PurchaseOrder> polist = await pocontext.ToListAsync();

                int PoId = polist[polist.Count - 1].PoId;

                foreach (string s in idlist)
                {
                    PODetails pods = new PODetails();
                    pods.ItemId = s;
                    pods.Item = itemContext.Where(p => p.ItemId == s).ToList().First();
                    pods.PoId = PoId;
                    pods.QtyOrdered = numlist[idlist.IndexOf(s)];
                    _context.Add(pods);
                    _context.SaveChanges();
                }
  
            }
            catch (Exception e)
            {
                System.Console.Write(e.ToString());
                return NotFound();
            }
            List<PODetails> podlist = new List<PODetails>();
            idlist = new List<string>();
            IsPosted = true;
            return RedirectToAction(nameof(ViewOrderHistory));
        }

        private bool PODetailsExists(int id)
        {
            return _context.Podetails.Any(e => e.PodetailsId == id);
        }
    }
}
