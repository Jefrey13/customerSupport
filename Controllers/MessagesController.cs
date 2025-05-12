using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CustomerService.API.Dtos.RequestDtos;
using CustomerService.API.Dtos.ResponseDtos;
using CustomerService.API.Services.Interfaces;
using CustomerService.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CustomerService.API.Controllers
{
    [ApiController]
    [Route("api/v1/conversations/{conversationId}/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messages;

        public MessagesController(IMessageService messages)
        {
            _messages = messages;
        }

        /// <summary>
        /// Retrieve all messages for a conversation (including attachments), ordered chronologically.
        /// </summary>
        [HttpGet(Name = "GetMessagesByConversation")]
        [SwaggerOperation(Summary = "Retrieve all messages for a conversation")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<MessageDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByConversation(
            [FromRoute] int conversationId,
            CancellationToken ct = default)
        {
            var list = await _messages.GetByConversationAsync(conversationId, ct);
            return Ok(new ApiResponse<IEnumerable<MessageDto>>(list, "Messages retrieved."));
        }

        /// <summary>
        /// Send a new message (text or media) in a conversation.
        /// </summary>
        [HttpPost(Name = "SendMessage")]
        [SwaggerOperation(Summary = "Send a message in a conversation")]
        [ProducesResponseType(typeof(ApiResponse<MessageDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Send(
            [FromRoute] int conversationId,
            [FromForm] SendMessageRequest req,
            CancellationToken ct = default)
        {
            req.ConversationId = conversationId;

            var dto = await _messages.SendMessageAsync(req, ct);
            return CreatedAtRoute(
                routeName: "GetMessagesByConversation",
                routeValues: new { conversationId = conversationId },
                value: new ApiResponse<MessageDto>(dto, "Message sent.")
            );
        }
    }
}
