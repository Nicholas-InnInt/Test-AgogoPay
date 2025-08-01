﻿using System;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Runtime.Session;
using Microsoft.AspNetCore.Mvc;
using Neptune.NsPay.Chat;
using Neptune.NsPay.Storage;
using Neptune.NsPay.Web.Controllers;

namespace Neptune.NsPay.Web.Areas.AppArea.Controllers
{
    [Area("AppArea")]
    [AbpMvcAuthorize]
    public class ChatController : ChatControllerBase
    {
        public ChatController(IBinaryObjectManager binaryObjectManager, IChatMessageManager chatMessageManager) :
            base(binaryObjectManager, chatMessageManager)
        {
        }

        public async Task<ActionResult> GetImage(int id, string contentType)
        {
            var message = await ChatMessageManager.FindMessageAsync(id, AbpSession.GetUserId());
            var jsonMessage = JsonNode.Parse(message.Message.Substring("[image]".Length));
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                var fileObject = await BinaryObjectManager.GetOrNullAsync(Guid.Parse(jsonMessage["id"].ToString()));
                if (fileObject == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }

                return File(fileObject.Bytes, contentType);
            }
        }

        public async Task<ActionResult> GetFile(int id, string contentType)
        {
            var message =await ChatMessageManager.FindMessageAsync(id, AbpSession.GetUserId());
            var jsonMessage = JsonNode.Parse(message.Message.Substring("[file]".Length));
            using (CurrentUnitOfWork.SetTenantId(null))
            {
                var fileObject = await BinaryObjectManager.GetOrNullAsync(Guid.Parse(jsonMessage["id"].ToString()));
                if (fileObject == null)
                {
                    return StatusCode((int)HttpStatusCode.NotFound);
                }

                return File(fileObject.Bytes, contentType);
            }
        }

        public PartialViewResult AddFriendModal()
        {
            return PartialView("_AddFriendModal");
        }

        public PartialViewResult AddFromDifferentTenantModal()
        {
            return PartialView("_AddFromDifferentTenantModal");
        }
    }
}
