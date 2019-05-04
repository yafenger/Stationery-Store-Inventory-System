using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ADProjectBase2.Models;
using Microsoft.EntityFrameworkCore;


namespace ADProjectBase2.Controllers
{

    public class HomeController : Controller
    {
        private readonly SSISContext _context;

        public HomeController(SSISContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {           
            return View();
        }

        //Show the department information the current user.
        public async Task<IActionResult> About()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                MyUser user = _context.MyUser.Where(p => p.Email == HttpContext.User.Identity.Name).Include(p => p.Dept).ToList().FirstOrDefault();
                int depID = user.DeptId;


                Department dep = await _context.Department.SingleOrDefaultAsync(d => d.DeptId == depID);

                int cpid = dep.Cpid;
                CollectionPoint cp = await _context.CollectionPoint.SingleOrDefaultAsync(c => c.Cpid == cpid);

                int? repId = dep.DeptRep;
                MyUser rep = await _context.MyUser.SingleOrDefaultAsync(u => u.UserId == repId);
                //*yafeng
                MyUser Head = await _context.MyUser.SingleOrDefaultAsync(u => u.DeptId == depID & u.RoleId == 2);
                MyUser actingHead = await _context.MyUser.SingleOrDefaultAsync(u => u.DeptId == depID & u.RoleId == 6);
                if (actingHead == null) actingHead = Head;
                ViewData["dep"] = dep;
                ViewData["cp"] = cp;
                ViewData["rep"] = rep;
                ViewData["Head"] = Head;
                ViewData["actingHead"] = actingHead;
                return View();
            }
            else return Redirect("~/Account/Login");
            
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
