using AutoMapper;
using DatingSiteAPIv2.DTO;
using DatingSiteAPIv2.Extensions;
using DatingSiteAPIv2.Helpers;
using DatingSiteAPIv2.Interface;
using DatingSiteAPIv2.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingSiteAPIv2.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;



        public MessagesController (IUserRepository userRepository, IMessageRepository message, IMapper mapper)
        {
            _userRepository = userRepository;
            _messageRepository = message;
            _mapper = mapper;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserClaims();

            if (username == createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You cannot send message to yourself");

            var sender = await _userRepository.GetUserByUsernameAsync(username);
            var recipient = await _userRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _messageRepository.AddMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok(_mapper.Map<MessageDto>(message));
            return BadRequest("Failed to send message");
        }

        [HttpGet]

        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUserClaims();

            var message = await _messageRepository.GetMessageForUser(messageParams);

            Response.AddPaginationHeader(message.CurrentPage, message.PageSize, message.TotalCount, message.TotalPages);

            return message;
        }

        [HttpGet("thread/{username}")]

        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username)
        {
            var currentUsername = User.GetUserClaims();

            return Ok(await _messageRepository.GetMessageThread(currentUsername, username));
        }


        [HttpDelete("{id}")]

        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserClaims();
            var message = await _messageRepository.GetMessage(id);

            if (message.Sender.UserName != username && message.Recipient.UserName != username) return Unauthorized();

            if (message.Sender.UserName == username) message.SenderDeleted = true;

            if (message.Recipient.UserName == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted) _messageRepository.DeleteMessage(message);

            if (await _messageRepository.SaveAllAsync()) return Ok();

            return BadRequest("Problem deleting the message");
        }

    }
}
