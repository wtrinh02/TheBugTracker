using Microsoft.EntityFrameworkCore;
using TheBugTracker.Data;
using TheBugTracker.Models;
using TheBugTracker.Services.Interfaces;

namespace TheBugTracker.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            //NEW Ticket has been Added
            if (oldTicket == null && newTicket != null) 
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    Property = "",
                    OldValue = "",
                    NewValue = "",
                    Created = DateTimeOffset.Now,
                    UserId = userId,
                    Description = "New Ticket Created"
                };

                try
                {
                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else 
            { 
                //Check Ticket Title Change
                if (oldTicket.Title != newTicket.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Title: {newTicket.Title}"
                    };

                    await _context.TicketHistories.AddAsync(history); 
                }

                //Check Ticket Description Change
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Description: {newTicket.Description}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                //Check Ticket Priority Change
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketPriority",
                        OldValue = oldTicket.TicketPriority.Name,
                        NewValue = newTicket.TicketPriority.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Priority: {newTicket.TicketPriority.Name}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                //Check Ticket Status Change
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketStatus",
                        OldValue = oldTicket.TicketStatus.Name,
                        NewValue = newTicket.TicketStatus.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Status: {newTicket.TicketStatus.Name}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                //Check Ticket Type Change
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "TicketType",
                        OldValue = oldTicket.TicketType.Name,
                        NewValue = newTicket.TicketType.Name,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Type: {newTicket.TicketType.Name}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                //Check Ticket Developer Change
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        Property = "Developer",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser?.FullName,
                        Created = DateTimeOffset.Now,
                        UserId = userId,
                        Description = $"New Ticket Developer: {newTicket.DeveloperUser.FullName}"
                    };

                    await _context.TicketHistories.AddAsync(history);
                }

                try
                {
                    //Save the TicketHistory to the Database
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            try
            {
                List<Project> projects = (await _context.Companies
                                                       .Include(c => c.Projects)
                                                            .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(t=> t.History)
                                                                    .ThenInclude(h => h.User)
                                                        .FirstOrDefaultAsync(c => c.Id == companyId)).Projects.ToList();

                List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();

                List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();

                return ticketHistories;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHitoriesAsync(int projectId, int companyId)
        {
            try
            {
                Project project = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                         .Include(p => p.Tickets)
                                                            .ThenInclude(t => t.History)
                                                                .ThenInclude(h => h.User)
                                                         .FirstOrDefaultAsync(p => p.Id == projectId);

                List<TicketHistory> histories = project.Tickets.SelectMany(t => t.History).ToList();

                return histories;

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
