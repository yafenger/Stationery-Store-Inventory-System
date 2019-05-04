using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ADProjectBase2.Models;

namespace ADProjectBase2.Controllers
{
    public class DeptRequestsController : Controller
    {
        private readonly SSISContext _context;
        static int count = 0;

        public DeptRequestsController(SSISContext context)
        {
            _context = context;
        }


        //GET:ConfirmedList for Store Clerk
        public async Task<ActionResult> ConfirmedList(int DeptId)
        {
            if (HttpContext.User.IsInRole("clerk"))
            {
                var ha = _context.DeptRequest.Include(x => x.Dept).Include(x => x.Item).Where(x => x.DeptId == DeptId & x.IsCompleted == false);
                return View(await ha.ToListAsync());
            }
            else return NotFound();
        }

        //POST ConfirmedList for Store Clerk
        [HttpPost]
        public async Task<ActionResult> ConfirmedList(List<DeptRequest> deptRequests)
        {
            int DeptId = 0;
            int deptRequestsCount = deptRequests.Count();
            for (int i = 0; i < deptRequestsCount; i++)
            {
                DeptRequest existing = _context.DeptRequest.Find(deptRequests[i].DeptReqId);
                DeptId = existing.DeptId;
                existing.IsCompleted = true;
                _context.Update(existing);
                await _context.SaveChangesAsync();
            }

            // to update IsComplete from "false" to "true" in RequestDetails table
            List<int> incompleteRequestsByDept = _context.Request.Where(x => x.User.DeptId == DeptId & x.Status == "Approved" & x.IsCompleted != true & x.Approvaltime < deptRequests[deptRequestsCount - 1].GeneratedTime).Select(x => x.RequestId).ToList();

            List<RequestDetails> initialList = new List<RequestDetails>();
            List<RequestDetails> incompleteReqDetailsByDept = new List<RequestDetails>();

            foreach (int i in incompleteRequestsByDept)
            {
                initialList = _context.RequestDetails.Where(x => x.RequestId == i & x.Type == "Order").ToList();
                incompleteReqDetailsByDept.AddRange(initialList);
            }

            foreach (RequestDetails rd in incompleteReqDetailsByDept)
            {
                rd.IsComplete = true;
                _context.Update(rd);
                await _context.SaveChangesAsync();
            }

            // to update IsComplete to "true" in Request table ( only if all the request details under this request Type is "order" and IsComplete is True

            List<int> retrieveRequestID = _context.Request.Where(x => x.Status == "Approved" & x.IsCompleted != true).Select(x => x.RequestId).ToList();

            List<RequestDetails> ha = new List<RequestDetails>();

            foreach (int i in retrieveRequestID)
            {
                ha = _context.RequestDetails.Where(x => x.RequestId == i & (x.Type != "Order" || x.IsComplete != true)).ToList();
                if (!ha.Any())
                {
                    Request r = _context.Request.Find(i);
                    r.IsCompleted = true;
                    _context.Update(r);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(ConfirmedList));
        }

        //GET:get the view of chargeback report
        public async Task<IActionResult> chargebacklist(string searchType,string searchString,int? year)
        {
            if (HttpContext.User.IsInRole("supervisor")||HttpContext.User.IsInRole("manager"))
            {
                List<String> stype = new List<string> { "ItemName", "Department" };

                DateTime st = new DateTime(DateTime.Now.Year, 1, 1);
                var chargeBack = from a in _context.DeptRequest.Include(p => p.Dept).Include(p => p.Item).Where(p => p.IsCompleted == true && p.TotalQty > p.ReceivedQty) select a;
                if (year == null)
                {
                    chargeBack = chargeBack.Where(p => (DateTime.Compare((DateTime)p.GeneratedTime, st) > 0));
                }
                else
                {
                    st = new DateTime((int)year, 1, 1);
                    DateTime et = new DateTime((int)year, 12, 31);
                    chargeBack = chargeBack.Where(p => (DateTime.Compare((DateTime)p.GeneratedTime, st) > 0) && (DateTime.Compare((DateTime)p.GeneratedTime, et) < 0));
                }
                if (!String.IsNullOrEmpty(searchString))
                {
                    if (!String.IsNullOrEmpty(searchType))
                    {
                        if (searchType == "ItemName") chargeBack = chargeBack.Where(s => s.Item.ItemName.Contains(searchString));
                        else if (searchType == "Department") chargeBack = chargeBack.Where(s => s.Dept.Name.Contains(searchString));
                    }
                }
                List<DeptRequest> chargebacklist = await chargeBack.ToListAsync();

                List<double> cbamount = new List<double>();
                double total = 0;
                foreach (DeptRequest dr in chargebacklist)
                {
                    double amount = (double)dr.Item.Unitprice * (double)(dr.TotalQty - dr.ReceivedQty);
                    cbamount.Add(amount);
                    total += amount;
                }
                var cbl = new ChargeBackModel
                {
                    deptRequests = chargebacklist,
                    deptList = new SelectList(stype),
                    amountList = cbamount,
                    tot = total,
                    startTime = st.ToString("Y")
                };
                return View(cbl);
            }
            else return NotFound();
        }


        //GET: get view or generate the consolidated list by department and by item.
        public async Task<IActionResult> Index([FromQuery(Name = "btnGenerate")]string buttonType,string searchType,string searchString)
        {
            if (HttpContext.User.IsInRole("clerk"))
            {
                if (buttonType == "generate")
                {
                    var pre = from a in _context.RequestDetails.Include(b => b.Request).Include(b => b.Item) select a;
                    pre = pre.Where(p => p.Request.IsCompleted == null && p.IsComplete == null && p.Type == "Order");

                    var items = from a in _context.Item select a;
                    var user = from a in _context.MyUser select a;
                    var deleteDeptr = from a in _context.DeptRequest where a.ReceivedQty == null select a;
                    List<DeptRequest> drlist = await deleteDeptr.ToListAsync();
                    foreach (DeptRequest dd in drlist)
                    {
                        _context.Remove(dd);
                        await _context.SaveChangesAsync();
                    }

                    List<RequestDetails> rdlist = await pre.ToListAsync();
                    List<Item> ilist = await items.ToListAsync();
                    List<MyUser> ulist = await user.ToListAsync();
                    List<DeptRequest> drlist2 = new List<DeptRequest>();
                    List<string> repeatDictionary = new List<string>();

                    foreach (RequestDetails rd in rdlist)
                    {
                        DeptRequest deptr = new DeptRequest();
                        foreach (MyUser u in ulist)
                        {
                            if (u.UserId == rd.Request.UserId)
                                deptr.DeptId = u.DeptId;
                        }
                        deptr.ItemId = rd.ItemId;
                        deptr.TotalQty = rd.RequestedQty;
                        deptr.ReceivedQty = null;
                        deptr.GeneratedTime = DateTime.Now;
                        if (repeatDictionary.IndexOf(deptr.DeptId + deptr.ItemId) < 0)
                        {
                            repeatDictionary.Add(deptr.DeptId + deptr.ItemId);
                            drlist2.Add(deptr);
                        }
                        else
                        {
                            drlist2.Add(deptr);
                        }
                    }

                    foreach (string s in repeatDictionary)
                    {
                        DeptRequest deptr = new DeptRequest();
                        deptr.TotalQty = 0;
                        foreach (DeptRequest dptr in drlist2)
                        {
                            if (dptr.DeptId + dptr.ItemId == s)
                            {
                                deptr.ItemId = dptr.ItemId;
                                deptr.DeptId = dptr.DeptId;
                                deptr.ReceivedQty = null;
                                deptr.GeneratedTime = DateTime.Now;
                                deptr.TotalQty += (int)dptr.TotalQty;
                            }
                        }
                        _context.Add(deptr);
                        await _context.SaveChangesAsync();
                    }
                    //retrieve from other tables and calculate the quantity within every department.
                    count++;
                }

                var dr = from m in _context.DeptRequest.Include(d => d.Item).Include(d => d.Dept).Include(d => d.Dept.Cp)
                         where m.ReceivedQty == null
                         select m;

                //searchbar controller
                List<String> stype = new List<string> { "ItemName", "Department","CollectionPoint"};

                if (!String.IsNullOrEmpty(searchString))
                {
                    if (!String.IsNullOrEmpty(searchType))
                    {
                        if (searchType == "ItemName") dr = dr.Where(s => s.Item.ItemName.Contains(searchString));
                        else if (searchType == "Department") dr = dr.Where(s => s.Dept.Name.Contains(searchString));
                        else if (searchType == "CollectionPoint") dr = dr.Where(s=>s.Dept.Cp.Name.Contains(searchString));
                    }
                }
                List<DeptRequest> drslist = await dr.ToListAsync();
                List<string> binnlist = new List<string>();
                List<string> drDictionary = new List<string>();
                if (drslist.Count > 0)
                {
                    foreach (DeptRequest d1 in drslist)
                    {
                        binnlist.Add("#"+d1.ItemId.Substring(1));
                        if (drDictionary.IndexOf(d1.ItemId)<0)
                        {
                            drDictionary.Add(d1.ItemId);
                        }
                    }
                }

                #region By Item
                //code here to get the consolidated list by item:
                List<GroupByItemView> bilist = new List<GroupByItemView>();
                foreach(string item in drDictionary)
                {
                    int total = 0;
                    foreach(DeptRequest d2 in drslist)
                    {
                        if (d2.ItemId == item) total += (int)d2.TotalQty;
                    }
                    GroupByItemView gbiv = new GroupByItemView();
                    gbiv.BinNumber = "#" + item.Substring(1);
                    gbiv.TotalQuantity = total;
                    gbiv.ItemName = _context.Item.Where(p => p.ItemId == item).FirstOrDefault().ItemName;
                    bilist.Add(gbiv);
                }
                #endregion

                var deptRequestsVM = new DeptRequestsViewModel
                {
                    deptList = new SelectList(stype),
                    deptRequests = drslist,
                    binlist = binnlist,
                    byItemList = bilist
                };
                return View(deptRequestsVM);
            }
            else return NotFound();
        }

        //GET:DeptList for the representative of his/her own department.
        public async Task<ActionResult> DeptList()
        {

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                MyUser user = _context.MyUser.Where(x => x.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int DeptRepDeptId = user.DeptId;
                var requestsByDept = _context.DeptRequest.Include(x => x.Dept).Include(x => x.Item).Where(x => x.DeptId == DeptRepDeptId & x.ReceivedQty == null);
                return View(await requestsByDept.ToListAsync());
            }
            else return NotFound();
        }


        //POST: deal with the confirm and the receive quantity of the list.
        [HttpPost]
        public async Task<IActionResult> DeptList(List<DeptRequest> deptRequests)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                MyUser user = _context.MyUser.Where(x => x.Email == HttpContext.User.Identity.Name).ToList().FirstOrDefault();
                int DeptRepDeptId = user.DeptId;
                int deptRequestsCount = deptRequests.Count();
                for (int i = 0; i < deptRequestsCount; i++)
                {
                    DeptRequest existing = _context.DeptRequest.Find(deptRequests[i].DeptReqId);
                    existing.ReceivedQty = deptRequests[i].ReceivedQty;
                    existing.IsCompleted = false;
                    _context.Update(existing);
                    await _context.SaveChangesAsync();
                }

                // to update IsComplete from "null" to "false" in RequestDetails table
                List<int> incompleteRequestsByDept = _context.Request.Where(x => x.User.DeptId == DeptRepDeptId & x.Status == "Approved" & x.IsCompleted != true & x.Approvaltime < deptRequests[deptRequestsCount - 1].GeneratedTime).Select(x => x.RequestId).ToList();

                List<RequestDetails> initialList = new List<RequestDetails>();
                List<RequestDetails> incompleteReqDetailsByDept = new List<RequestDetails>();

                foreach (int i in incompleteRequestsByDept)
                {
                    initialList = _context.RequestDetails.Where(x => x.RequestId == i & x.Type == "Order").ToList();
                    incompleteReqDetailsByDept.AddRange(initialList);
                }

                foreach (RequestDetails rd in incompleteReqDetailsByDept)
                {
                    rd.IsComplete = false;
                    _context.Update(rd);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(DeptList));
        }
 
        private bool DeptRequestExists(int id)
        {
            return _context.DeptRequest.Any(e => e.DeptReqId == id);
        }
    }
}
