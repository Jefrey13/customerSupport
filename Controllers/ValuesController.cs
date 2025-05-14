using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Repositories.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _uow;

        public DashboardController(IUnitOfWork uow)
            => _uow = uow;

        // 1. Cards de resumen

        /// <summary>Count de conversaciones cuyo status != "Closed"</summary>
        [HttpGet("ActiveConversations")]
        public async Task<ActionResult<ApiResponse<int>>> GetActiveConversationsCount()
        {
            var count = await _uow.Conversations
                .GetAll()
                .CountAsync(c => c.Status != "Closed");
            return Ok(new ApiResponse<int>(count));
        }

        /// <summary>Count de conversaciones iniciadas en la fecha indicada (default = hoy)</summary>
        [HttpGet("NewConversations")]
        public async Task<ActionResult<ApiResponse<int>>> GetNewConversationsCount(
            [FromQuery] DateTime? date = null)
        {
            var target = date?.Date ?? DateTime.UtcNow.Date;
            var count = await _uow.Conversations
                .GetAll()
                .CountAsync(c => c.CreatedAt.Date == target);
            return Ok(new ApiResponse<int>(count));
        }

        /// <summary>Count de agentes actualmente en línea (definido por lógica de negocio)</summary>
        [HttpGet("OnlineAgentsCount")]
        public async Task<ActionResult<ApiResponse<int>>> GetOnlineAgentsCount()
        {
            // Asumimos que "online" se determina por última actividad < 5 minutos
            var threshold = DateTime.UtcNow.AddMinutes(-5);
            var count = await _uow.Users
                .GetAll()
                .CountAsync(u => u.IsActive
                                && u.UserRoles.Any(ur => ur.Role.RoleName == "Agent")
                                && u.UpdatedAt >= threshold);
            return Ok(new ApiResponse<int>(count));
        }

        // 2. Series temporales y breakdowns

        /// <summary>List of { date, count } agrupado por día</summary>
        [HttpGet("ConversationsPerDay")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TemporalCountDto>>>> GetConversationsPerDay(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var query = _uow.Conversations.GetAll()
                         .Where(c => c.CreatedAt.Date >= from.Date
                                  && c.CreatedAt.Date <= to.Date);

            var list = await query
                .GroupBy(c => c.CreatedAt.Date)
                .Select(g => new TemporalCountDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<TemporalCountDto>>(list));
        }

        /// <summary>
        /// Dos series: enviados y recibidos por día
        /// </summary>
        [HttpGet("MessagesSentReceived")]
        public async Task<ActionResult<ApiResponse<MessagesSentReceivedDto>>> GetMessagesSentReceived(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            // mensajes enviados = MessageType == "Outgoing"
            // mensajes recibidos = MessageType == "Incoming"
            var baseQ = _uow.Messages.GetAll()
                         .Where(m => m.CreatedAt.Date >= from.Date
                                  && m.CreatedAt.Date <= to.Date);

            var sent = await baseQ
                .Where(m => m.MessageType == "Outgoing")
                .GroupBy(m => m.CreatedAt.Date)
                .Select(g => new TemporalCountDto { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            var received = await baseQ
                .Where(m => m.MessageType == "Incoming")
                .GroupBy(m => m.CreatedAt.Date)
                .Select(g => new TemporalCountDto { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(new ApiResponse<MessagesSentReceivedDto>(
                new MessagesSentReceivedDto
                {
                    Sent = sent,
                    Received = received
                }));
        }

        /// <summary>
        /// Usuarios activos por día: usuarios distintos que participaron en al menos un mensaje
        /// </summary>
        [HttpGet("ActiveUsersPerDay")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TemporalCountDto>>>> GetActiveUsersPerDay(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var msgQ = _uow.Messages.GetAll()
                        .Where(m => m.CreatedAt.Date >= from.Date
                                 && m.CreatedAt.Date <= to.Date);

            var list = await msgQ
                .GroupBy(m => m.CreatedAt.Date)
                .Select(g => new TemporalCountDto
                {
                    Date = g.Key,
                    Count = g.Select(m => m.SenderId).Distinct().Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(new ApiResponse<IEnumerable<TemporalCountDto>>(list));
        }
    }

}
