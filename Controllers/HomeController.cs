using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TheBugTracker.Extensions;
using TheBugTracker.Models;
using TheBugTracker.Models.ChartModels;
using TheBugTracker.Models.Enums;
using TheBugTracker.Models.ViewModels;
using TheBugTracker.Services;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTCompanyInfoService _companyInfoService;
        private readonly IBTProjectService _projectService;
		private readonly IBTRolesService _rolesService;
		private readonly IBTTicketService _ticketService;

		public HomeController(ILogger<HomeController> logger, IBTCompanyInfoService companyInfoService, IBTProjectService projectService, IBTRolesService rolesService, IBTTicketService ticketService)
		{
			_logger = logger;
			_companyInfoService = companyInfoService;
			_projectService = projectService;
			_rolesService = rolesService;
			_ticketService = ticketService;
		}

		public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Dashboard");
            }

            return View();
        }

        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity.GetCompanyId().Value;
           
            model.Company = await _companyInfoService.GetCompanyInfoByIdAsync(companyId);
            model.Projects = (await _companyInfoService.GetAllProjectsAsync(companyId)).Where(p=> p.Archived == false).ToList();
            model.Tickets = model.Projects.SelectMany(p => p.Tickets).Where(t=> t.Archived == false).ToList();
            model.Members = await _companyInfoService.GetAllMembersAsync(companyId);

			int count = 0;
			foreach (Project project in model.Projects)
			{
				if (await _projectService.GetProjectManagerAsync(project.Id) == null)
				{
					count++;
				}
			}
			ViewData["count"] = count;
			ViewData["numAdmin"] = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Admin), companyId)).Count();
			ViewData["numPM"] = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.ProjectManager), companyId)).Count(); 
			ViewData["numDev"] = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Developer), companyId)).Count();
			ViewData["numSub"] = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Submitter), companyId)).Count(); 



			return View(model);
        }

		
        public async Task<IActionResult> SortProjects()
		{
            return RedirectToAction("Dashboard", "Home",  "projectSection");
        }


        [HttpPost]
		public async Task<JsonResult> GglProjectTickets()
		{
			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "ProjectName", "TicketCount" });

			foreach (Project prj in projects)
			{
				chartData.Add(new object[] { prj.Name, prj.Tickets.Count() });
			}

			return Json(chartData);
		}

		[HttpPost]
		public async Task<JsonResult> GglTicketPriority()
		{
			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);
			List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyAsync(companyId);

			List<object> chartData = new();
			chartData.Add(new object[] { "Priority", "Count" });


			foreach (string priority in Enum.GetNames(typeof(BTTicketPriority)))
			{
				int priorityCount = (await _ticketService.GetAllTicketsByPriorityAsync(companyId, priority)).Count();
				chartData.Add(new object[] { priority, priorityCount });
			}

			return Json(chartData);
		}

		[HttpPost]
		public async Task<JsonResult> AmCharts()
		{

			AmChartData amChartData = new();
			List<AmItem> amItems = new();

			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = (await _companyInfoService.GetAllProjectsAsync(companyId)).Where(p => p.Archived == false).ToList();

			foreach (Project project in projects)
			{
				AmItem item = new();

				item.Project = project.Name;
				item.Tickets = project.Tickets.Count;
				item.Developers = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(Roles.Developer))).Count();

				amItems.Add(item);
			}

			amChartData.Data = amItems.ToArray();


			return Json(amChartData.Data);
		}

		[HttpPost]
		public async Task<JsonResult> PlotlyBarChart()
		{
			PlotlyBarData plotlyData = new();
			List<PlotlyBar> barData = new();

			int companyId = User.Identity.GetCompanyId().Value;

			List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

			//Bar One
			PlotlyBar barOne = new()
			{
				X = projects.Select(p => p.Name).ToArray(),
				Y = projects.SelectMany(p => p.Tickets).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
				Name = "Tickets",
				Type = "bar"
			};

			//Bar Two
			PlotlyBar barTwo = new()
			{
				X = projects.Select(p => p.Name).ToArray(),
				Y = projects.Select(async p => (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(Roles.Developer))).Count).Select(c => c.Result).ToArray(),
				Name = "Developers",
				Type = "bar"
			};

			barData.Add(barOne);
			barData.Add(barTwo);

			plotlyData.Data = barData;

			return Json(plotlyData);
		}

		public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
