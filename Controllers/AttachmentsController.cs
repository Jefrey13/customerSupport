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
    [Route("api/v1/[controller]")]
    public class AttachmentsController : ControllerBase
    {
        private readonly IAttachmentService _attachments;

        public AttachmentsController(IAttachmentService attachments)
        {
            _attachments = attachments;
        }

        [HttpGet("message/{messageId}", Name = "GetAttachmentsByMessage")]
        [SwaggerOperation(Summary = "Retrieve attachments for a given message")]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AttachmentDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByMessage(
            [FromRoute] int messageId,
            CancellationToken ct = default)
        {
            var list = await _attachments.GetByMessageAsync(messageId, ct);
            return Ok(new ApiResponse<IEnumerable<AttachmentDto>>(list, "Attachments retrieved."));
        }

        [HttpPost(Name = "UploadAttachment")]
        [DisableRequestSizeLimit]
        [SwaggerOperation(Summary = "Upload a new attachment for a message")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<AttachmentDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Upload(
            [FromForm] UploadAttachmentRequest req,
            CancellationToken ct = default)
        {
            var dto = await _attachments.UploadAsync(req, ct);
            return CreatedAtRoute(
                routeName: "GetAttachmentsByMessage",
                routeValues: new { messageId = dto.MessageId },
                value: new ApiResponse<AttachmentDto>(dto, "Attachment uploaded.")
            );
        }
    }
}