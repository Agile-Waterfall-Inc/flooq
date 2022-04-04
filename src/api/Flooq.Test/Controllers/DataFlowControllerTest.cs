﻿using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Flooq.Api.Controllers;
using Flooq.Api.Models;
using Flooq.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Flooq.Test.Controllers;

[TestClass]
public class DataFlowControllerTest
{
  private readonly Mock<IDataFlowService> _serviceMock = new Mock<IDataFlowService>();
  
  [TestMethod]
  public void CanCreateDataFlowController()
  {
    var mockDataFlowService = _serviceMock.Object;
    var dataFlowController = new DataFlowController(mockDataFlowService);
    Assert.IsNotNull(dataFlowController);
  }

  [TestMethod]
  public void CanGetDataFlows()
  {
    var demoDataFlow = new DataFlow
    {
      Id = Guid.NewGuid(),
      Name = "Demo Flow",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };
    
    var dataFlows = new List<DataFlow>();
    dataFlows.Add(demoDataFlow);
    
    var actionResult = new ActionResult<IEnumerable<DataFlow>>(dataFlows);
    _serviceMock.Setup(service => service.GetDataFlows()).ReturnsAsync(actionResult);

    var dataFlowController = new DataFlowController(_serviceMock.Object);
    
    Assert.AreSame(actionResult, dataFlowController.GetDataFlows().Result);
  }

  [TestMethod]
  public void CanGetDataFlow()
  {
    var demoDataFlow = new DataFlow
    {
      Id = Guid.NewGuid(),
      Name = "Demo Flow",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };

    _serviceMock.Setup(service => service.GetDataFlow(demoDataFlow.Id)).ReturnsAsync(demoDataFlow);

    var dataFlowController = new DataFlowController(_serviceMock.Object);

    Assert.AreSame(demoDataFlow, dataFlowController.GetDataFlow(demoDataFlow.Id).Result.Value);
  }

  [TestMethod]
  public void CanPutDataFlow()
  {
    var demoDataFlow = new DataFlow
    {
      Id = Guid.NewGuid(),
      Name = "Demo Flow",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };

    var dataFlowController = new DataFlowController(_serviceMock.Object);

    var result = dataFlowController.PutDataFlow(demoDataFlow.Id, demoDataFlow);
    Assert.IsNotNull(result.Result);
    Assert.AreEqual(new NoContentResult().ToString(), result.Result.ToString());
  }

  [TestMethod]
  public void CanPostDataFlow()
  {
    var demoDataFlow = new DataFlow
    {
      Id = Guid.NewGuid(),
      Name = "Demo Flow",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };

    var dataFlowController = new DataFlowController(_serviceMock.Object);

    var result = dataFlowController.PostDataFlow(demoDataFlow);
    Assert.IsNotNull(result.Result.Result);
  }
}