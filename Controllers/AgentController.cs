using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ADProjectBase2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ADProjectBase1.Controllers
{
    public class AgentController : Controller
    {
        private readonly SSISContext _context;
        public AgentController(SSISContext context)
        {
            _context = context;
        }
        //GET
        public IActionResult Dashboard()
        {
            if (HttpContext.User.Identity.IsAuthenticated &&(HttpContext.User.IsInRole("manager") || HttpContext.User.IsInRole("supervisor")))
            {
                ViewData["Dept"] = new SelectList(_context.Department, "Name", "Name");
                ViewData["Cate1"] = new SelectList(_context.Category, "CatName", "CatName");
                ViewData["Cate2"] = new SelectList(_context.Category, "CatName", "CatName");
                ViewData["Cate3"] = new SelectList(_context.Category, "CatName", "CatName");
                List<string> yearl = new List<string> {(DateTime.Now.Year-2).ToString(), (DateTime.Now.Year - 1).ToString(), (DateTime.Now.Year).ToString() };
                ViewBag.Year1 = new SelectList(yearl);
                ViewBag.Year2 = new SelectList(yearl);
                ViewBag.Year3 = new SelectList(yearl);
                List<string> monthTl = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sep", "Oct", "Nov", "Dec" };
                List<string> monthVl = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
                ViewData["Month1"] = new SelectList(monthTl);
                ViewData["Month2"] = new SelectList(monthTl);
                ViewData["Month3"] = new SelectList(monthTl);

                return View();
            }
            return NotFound();
        }
        //POST:generate trend report by category
        [HttpPost]
        public IActionResult Dashboard(string year1, string month1, string year2, string month2, string year3, string month3, string dept, string categories1, string categories2, string categories3)
        {
            ViewData["Dept"] = new SelectList(_context.Department, "Name", "Name", dept);
            ViewData["Cate1"] = new SelectList(_context.Category, "CatName", "CatName", categories1);
            ViewData["Cate2"] = new SelectList(_context.Category, "CatName", "CatName", categories2);
            ViewData["Cate3"] = new SelectList(_context.Category, "CatName", "CatName", categories3);
            List<string> yearl = new List<string> { (DateTime.Now.Year - 2).ToString(), (DateTime.Now.Year - 1).ToString(), (DateTime.Now.Year).ToString() };

            ViewBag.Year1 = new SelectList(yearl, year1);
            ViewBag.Year2 = new SelectList(yearl, year2);
            ViewBag.Year3 = new SelectList(yearl, year3);
            List<string> monthTl = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "June", "July", "Aug", "Sep", "Oct", "Nov", "Dec" };
            List<string> monthVl = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
            ViewData["Month1"] = new SelectList(monthTl, month1);
            ViewData["Month2"] = new SelectList(monthTl, month2);
            ViewData["Month3"] = new SelectList(monthTl, month3);

            for (int i = 0; i < 12; i++)
            {
                if (month1.Equals(monthTl[i]))
                    month1 = (i+1).ToString();
            }
            for (int i = 0; i < 12; i++)
            {
                if (month2.Equals(monthTl[i]))
                    month2 = (i+1).ToString();
            }
            for (int i = 0; i < 12; i++)
            {
                if (month3.Equals(monthTl[i]))
                    month3 = (i+1).ToString();
            }
            //**bar chart legend value
            ViewBag.Date1 = year1 + "-" + month1 + " ";
            ViewBag.Date2 = year2 + "-" + month2 + " ";
            ViewBag.Date3 = year3 + "-" + month3 + " ";
            //**
            //**line chart legend value
            ViewBag.lDept1 = categories1 + " ";
            ViewBag.lDept2 = categories2 + " ";
            ViewBag.lDept3 = categories3 + " ";
            //**line chart x-value
            string lDate1 = year1 + "-" + month1;
            string lDate2 = year2 + "-" + month2;
            string lDate3 = year3 + "-" + month3;
            List<string> lDate = new List<string> { lDate1, lDate2, lDate3 };
            var xlvalue = lDate;
            ViewBag.xlvalue = xlvalue;


            //***
            List<string> xlist = new List<string>();

            xlist.Add(categories1);
            xlist.Add(categories2);
            xlist.Add(categories3);
           
            string n;
            n = dept;

            DateTime dateTime = new DateTime(2019, 1, 1);

            List<string> y = new List<string> { year1, year2, year3 };
            List<string> m = new List<string> { month1, month2, month3 };
            var deptReqlist = from x in _context.DeptRequest.Include(p => p.Dept).Include(q => q.Item)
                              .Where(x => x.Dept.Name.Contains(n))
                              select x;
            List<DeptRequest> ilist = deptReqlist.ToList();
            List<int> qtyl = new List<int> { 0, 0, 0 };
            List<int> qtyl2 = new List<int> { 0, 0, 0 };
            List<int> qtyl3 = new List<int> { 0, 0, 0 };
            List<List<int>> Qty = new List<List<int>> { qtyl, qtyl2, qtyl3 };
            foreach (DeptRequest dr in ilist)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (dr.Item.ItemName.Contains(xlist[i]))
                    {
                        dateTime = dr.GeneratedTime.Value;
                        for (int j = 0; j < 3; j++)
                        {
                            if (dateTime.Month.ToString() == m[j] && dateTime.Year.ToString() == y[j])
                            {
                                Qty[j][i] += (int)dr.TotalQty;
                            }
                        }
                    }
                }
            }
            var xV = xlist;
            var yV = qtyl;
            var yV2 = qtyl2;
            var yV3 = qtyl3;
            ViewBag.xvalue = xV;
            ViewBag.yvalue = yV;
            ViewBag.yvalue2 = yV2;
            ViewBag.yvalue3 = yV3;

            //***line chart y-value
            //**line chart yvalue
            List<int> lqtyl = new List<int> { 0, 0, 0 };
            List<int> lqtyl2 = new List<int> { 0, 0, 0 };
            List<int> lqtyl3 = new List<int> { 0, 0, 0 };
            List<List<int>> lQty = new List<List<int>> { lqtyl, lqtyl2, lqtyl3 };
            foreach (DeptRequest dr in ilist)
            {
                for (int i = 0; i < 3; i++)
                {// 3 categories
                    if (dr.Item.ItemName.Contains(xlist[i]))
                    {
                        dateTime = dr.GeneratedTime.Value;
                        for (int j = 0; j < 3; j++)
                        {// 3 months
                            if (dateTime.Month.ToString() == m[j] && dateTime.Year.ToString() == y[j])
                            {
                                lQty[i][j] += (int)dr.TotalQty;
                            }
                        }
                    }
                }
            }
            var lyV = lqtyl;
            var lyV2 = lqtyl2;
            var lyV3 = lqtyl3;
            ViewBag.ylvalue = lyV;
            ViewBag.ylvalue2 = lyV2;
            ViewBag.ylvalue3 = lyV3;

            return View();
        }
    }
}