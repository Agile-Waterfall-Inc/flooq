using Flooq.Api.Metrics.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flooq.Api.Models;
using Flooq.Api.Services;

namespace Flooq.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DataFlowController : ControllerBase
    { 
        private readonly IDataFlowService _dataFlowService;
        private readonly ILinearizedGraphService _graphService;
        private readonly IDataFlowMetricsService _dataFlowMetricsService;

        public DataFlowController(IDataFlowService dataFlowService, ILinearizedGraphService graphService, IDataFlowMetricsService dataFlowMetricsService)
        { 
          _dataFlowService = dataFlowService;
          _graphService = graphService;
          _dataFlowMetricsService = dataFlowMetricsService;
        }

        // GET: api/DataFlow
        /// <summary>
        /// Gets every <see cref="DataFlow"/>.
        /// </summary>
        /// <returns>Every <see cref="DataFlow"/></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DataFlow>>> GetDataFlows()
        {
          _dataFlowMetricsService.IncrementRequestedListsCount();
          return await _dataFlowService.GetDataFlows();
        }

        // GET: api/DataFlow/5
        /// <summary>
        /// Gets a specific <see cref="DataFlow"/> by id.
        /// </summary>
        /// <param name="id">Identifies the specific <see cref="DataFlow"/>.</param>
        /// <returns>
        /// The specific <see cref="DataFlow"/>
        /// or <see cref="NotFoundResult"/> if no <see cref="DataFlow"/> was identified by the id.
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<DataFlow?>> GetDataFlow(Guid? id)
        {
          var actionResult = await _dataFlowService.GetDataFlow(id);

          if (actionResult.Value == null)
          {
            _dataFlowMetricsService.IncrementNotFoundCount();
            return NotFound();
          }

          _dataFlowMetricsService.IncrementRequestedByIdCount();
          return actionResult;
        }

        // PUT: api/DataFlow/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Overrides a specific <see cref="DataFlow"/> with a new <see cref="DataFlow"/>.
        /// Parameter id has to match the id of the put <see cref="DataFlow"/>.
        /// Even if not null, the field lastEdited will be ignored. Instead, it's automatically updated.
        /// </summary>
        /// <param name="id">Identifies the specific <see cref="DataFlow"/>. Has to match the id of the new <see cref="DataFlow"/>.</param>
        /// <param name="dataFlow">The new <see cref="DataFlow"/>. Its id has to match the parameter id.</param>
        /// <returns>The specific <see cref="DataFlow"/>
        /// or <see cref="BadRequestResult"/> if ids of do not match
        /// or <see cref="NotFoundResult"/> if no <see cref="DataFlow"/> was identified by the id.</returns>
        [HttpPut("{id}")]
        public async Task<ActionResult<DataFlow>> PutDataFlow(Guid? id, DataFlow dataFlow)
        {
            if (id == null || id != dataFlow.Id)
            {
              _dataFlowMetricsService.IncrementBadRequestCount();
              return BadRequest();
            }

            dataFlow.LastEdited = DateTime.UtcNow;

            var actionResultDataFlow = _dataFlowService.PutDataFlow(dataFlow);

            try
            {
                await _dataFlowService.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DataFlowExists(id))
                {
                  _dataFlowMetricsService.IncrementNotFoundCount();
                  return NotFound();
                }
                _dataFlowMetricsService.IncrementExceptionCount();
                throw;
            }
            
            // Delete LinearizedGraph of changed DataFlow
            var actionResultGraph = await _graphService.GetGraph(id.Value);
            var graph = actionResultGraph?.Value; // Conditional access qualifier is needed!
            if (graph != null)
            {
              _graphService.RemoveGraph(graph);
              await _graphService.SaveChangesAsync();
            }

            _dataFlowMetricsService.IncrementEditedCount();
            return actionResultDataFlow;
        }

        // POST: api/DataFlow
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        /// <summary>
        /// Adds a <see cref="DataFlow"/>.
        /// If null, the uuid is automatically created and set.
        /// If null, the status is set to "Disabled".
        /// Even if not null, the field lastEdited will be ignored. Instead, it's automatically created.
        /// </summary>
        /// <param name="dataFlow">The new <see cref="DataFlow"/>.</param>
        /// <returns>A <see cref="CreatedAtActionResult"/> object that produces a <see cref="StatusCodes.Status201Created"/> response.</returns>
        [HttpPost]
        public async Task<ActionResult<DataFlow>> PostDataFlow(DataFlow dataFlow)
        {
          if (DataFlowExists(dataFlow.Id))
          {
            _dataFlowMetricsService.IncrementBadRequestCount();
            return BadRequest();
          }
          
          dataFlow.LastEdited = DateTime.UtcNow;
          
          _dataFlowService.AddDataFlow(dataFlow);
          await _dataFlowService.SaveChangesAsync();

          _dataFlowMetricsService.IncrementCreatedCount();
          return CreatedAtAction(nameof(GetDataFlow), new { id = dataFlow.Id }, dataFlow);
        }
        
        // DELETE: api/DataFlow/5
        /// <summary>
        /// Deletes a specific <see cref="DataFlow"/>.
        /// </summary>
        /// <param name="id">Identifies the specific <see cref="DataFlow"/>.</param>
        /// <returns>
        /// <see cref="NoContentResult"/> if deletion was successful
        /// or <see cref="NotFoundResult"/> if no <see cref="DataFlow"/> was identified by the id.
        /// </returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDataFlow(Guid? id)
        {
            var actionResult = await _dataFlowService.GetDataFlow(id);
            var dataFlow = actionResult?.Value; // Conditional access qualifier is needed!
            
            if (dataFlow == null)
            {
              _dataFlowMetricsService.IncrementNotFoundCount();
              return NotFound();
            }

            _dataFlowService.RemoveDataFlow(dataFlow);
            await _dataFlowService.SaveChangesAsync();

            _dataFlowMetricsService.IncrementDeletedCount();
            return NoContent();
        }

        private bool DataFlowExists(Guid? id)
        {
            return _dataFlowService.DataFlowExists(id);
        }
    }
}
