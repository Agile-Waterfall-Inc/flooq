using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flooq.Api.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Factory = Flooq.IntegrationTest.FlooqWebApplicationFactory<Program>;

namespace Flooq.IntegrationTest;

[TestClass]
public class DataFlowTest
{
  private HttpClient _client;
  [TestInitialize]
  public void Setup()
  {
    _client = Factory.Factory.CreateClient();
  }
  
  [TestMethod]
  public async Task CanGetDataFlows()
  {
    var response = await _client.GetAsync("api/DataFlow");

    response.EnsureSuccessStatusCode();
    var dataFlows = response.Content.ReadAsStringAsync().Result;
    
    Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    Assert.IsTrue(dataFlows.Contains(Factory.TEST_GUID.ToString()));
  }

  [TestMethod]
  public async Task CanGetDataFlow()
  {
    var response = await _client.GetAsync($"api/DataFlow/{Factory.TEST_GUID}");

    response.EnsureSuccessStatusCode();
    var dataFlows = response.Content.ReadAsStringAsync().Result;
    
    Assert.AreEqual("application/json; charset=utf-8", response.Content.Headers.ContentType.ToString());
    Assert.IsTrue(dataFlows.Contains(Factory.TEST_GUID.ToString()));
  }

  [TestMethod]
  public async Task CannotGetNonExistingDataFlow()
  {
    var response = await _client.GetAsync($"api/DataFlow/{Guid.NewGuid()}");

    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
  }

  [TestMethod]
  public async Task CanPostDataFlow()
  {
    var dataFlow = new DataFlow()
    {
      Id = Guid.NewGuid(),
      Name = "Demo Flow",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };

    var response = await _client.PostAsync("api/DataFlow", new StringContent(dataFlow.ToString()));

    response.EnsureSuccessStatusCode();
    Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
  }

  [TestMethod]
  public async Task CanPutDataFlow()
  {
    var dataFlow = new DataFlow()
    {
      Id = Factory.TEST_GUID,
      Name = "Demo Flow 2",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };

    var response = await _client.PutAsync($"api/DataFlow/{Factory.TEST_GUID}", 
      new StringContent(dataFlow.ToString()));

    response.EnsureSuccessStatusCode();
    Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
  }

  [TestMethod]
  public async Task CannotPutNonExistingDataFlow()
  {
    var id = Guid.NewGuid();
    var dataFlow = new DataFlow()
    {
      Id = id,
      Name = "Demo Flow 2",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };

    var response = await _client.PutAsync($"api/DataFlow/{id}", new StringContent(dataFlow.ToString()));
    
    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
  }

  [TestMethod]
  public async Task CannotPutDataFlowWithWrongId()
  {
    var id = Guid.NewGuid();
    var dataFlow = new DataFlow()
    {
      Id = id,
      Name = "Demo Flow 2",
      Status = "Active",
      LastEdited = DateTime.Now,
      Definition = "{}"
    };

    var response = await _client.PutAsync($"api/DataFlow/{Factory.TEST_GUID}", 
      new StringContent(dataFlow.ToString()));
    
    Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [TestMethod]
  public async Task CanDeleteDataFlow()
  {
    var response = await _client.DeleteAsync($"api/DataFlow/{Factory.TEST_GUID}");

    response.EnsureSuccessStatusCode();
    Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
  }

  [TestMethod]
  public async Task CannotDeleteNonExistingDataFlow()
  {
    var response = await _client.DeleteAsync($"api/DataFlow/{Guid.NewGuid()}");
    
    Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
  }
}