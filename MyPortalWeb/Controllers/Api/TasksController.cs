using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MyPortal.Database.Models.Search;
using MyPortal.Logic.Extensions;
using MyPortal.Logic.Interfaces.Services;
using MyPortal.Logic.Models.Data.People;
using MyPortal.Logic.Models.Requests.Person.Tasks;
using MyPortalWeb.Models.Requests;

namespace MyPortalWeb.Controllers.Api
{
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        [HttpGet]
        [Route("api/tasks/{taskId}")]
        [ProducesResponseType(typeof(TaskModel), 200)]
        public async Task<IActionResult> GetById([FromRoute] Guid taskId)
        {
            var task = await _taskService.GetTaskById(taskId);

            return Ok(task);
        }

        [HttpGet]
        [Route("api/people/{personId}/tasks")]
        [ProducesResponseType(typeof(IEnumerable<TaskModel>), 200)]
        public async Task<IActionResult> GetByPerson([FromRoute] Guid personId,
            [FromQuery] TaskSearchOptions searchOptions)
        {
            var tasks = (await _taskService.GetByPerson(personId, searchOptions)).ToArray();

            return Ok(tasks);
        }

        [HttpGet]
        [Route("api/tasks/types")]
        [ProducesResponseType(typeof(IEnumerable<TaskTypeModel>), 200)]
        public async Task<IActionResult> GetTaskTypes([FromQuery] bool personal = false)
        {
            var taskTypes = await _taskService.GetTypes(personal);

            return Ok(taskTypes);
        }

        [HttpPost]
        [Route("api/tasks")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Create([FromBody] TaskRequestModel requestModel)
        {
            var userId = User.GetUserId();

            if (userId != null)
            {
                requestModel.AssignedById = userId.Value;

                await _taskService.CreateTask(requestModel);

                return Ok();
            }

            return Unauthorized();
        }

        [HttpPut]
        [Route("api/tasks/{taskId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Update([FromRoute] Guid taskId, [FromBody] TaskRequestModel requestModel)
        {
            await _taskService.UpdateTask(taskId, requestModel);

            return Ok();
        }

        [HttpPost]
        [Route("api/tasks/{taskId}/complete")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> ToggleCompleted([FromBody] TaskToggleRequestModel model)
        {
            await _taskService.SetCompleted(model.TaskId, model.Completed);

            return Ok();
        }

        [HttpDelete]
        [Route("api/tasks/{taskId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] Guid taskId)
        {
            await _taskService.DeleteTask(taskId);

            return Ok();
        }
    }
}