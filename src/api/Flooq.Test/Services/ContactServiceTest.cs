using System;
using System.Linq;
using System.Threading.Tasks;
using Flooq.Api.Domain;
using Flooq.Api.Models;
using Flooq.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Flooq.Test.Services;

[TestClass]
public class ContactServiceTest
{
  private FlooqContext? _context;
  private int _n;

  private readonly Contact _contact = new("test@example.com");

  private readonly Random _random = new();

  [TestInitialize]
  public async Task Setup()
  {
    var config = new ConfigurationManager();
    config.AddJsonFile("appsettings.Test.json");
    _context = new FlooqContext(
      new DbContextOptionsBuilder<FlooqContext>().UseInMemoryDatabase(databaseName: "FlooqDatabase").Options, config);
    
    foreach (var contact in _context.Contacts) _context.Contacts.Remove(contact);
    await _context.SaveChangesAsync();
    _n = _random.Next(2, 11);
  }

  [TestMethod]
  public void CanCreateContactService()
  {
    Assert.IsNotNull(_context?.Contacts);
    var contactService = new ContactService(_context);
    Assert.IsNotNull(contactService);
  }

  [TestMethod]
  public async Task CanGetContacts_Zero()
  {
    var contactService = new ContactService(_context!);

    var receivedActionResult = await contactService.GetContacts();
    Assert.AreEqual(0, receivedActionResult.Value?.Count());
  }

  [TestMethod]
  public async Task CanGetContacts_One()
  {
    var contactService = new ContactService(_context!);

    _context?.Contacts.Add(_contact);
    await _context?.SaveChangesAsync()!;

    var receivedActionResult = await contactService.GetContacts();
    Assert.AreEqual(1, receivedActionResult.Value?.Count());
  }

  [TestMethod]
  public async Task CanGetContacts_Multiple()
  {
    var contactService = new ContactService(_context!);

    for (int i = 0; i < _n; i++)
    {
      _context?.Contacts.Add(new Contact(i + "example.com"));
    }

    await _context?.SaveChangesAsync()!;

    var receivedActionResult = await contactService.GetContacts();
    Assert.AreEqual(_n, receivedActionResult.Value?.Count());
  }

  [TestMethod]
  public async Task CanGetContactById()
  {
    var contactService = new ContactService(_context!);

    _context?.Contacts.Add(_contact);
    await _context?.SaveChangesAsync()!;

    var receivedActionResult = await contactService.GetContact(_contact.Email);
    var contact = receivedActionResult.Value;

    Assert.AreSame(_contact, contact);
  }
  
  [TestMethod]
  public async Task CanAddContactAndSaveChangesAsync()
  {
    var contactService = new ContactService(_context!);

    var receivedActionResult = await contactService.GetContact(_contact.Email);
    var contact = receivedActionResult.Value;
    Assert.IsNull(contact);
    
    contactService.AddContact(_contact);
    await contactService.SaveChangesAsync();
    
    receivedActionResult = await contactService.GetContact(_contact.Email);
    contact = receivedActionResult.Value;
    Assert.IsNotNull(contact);
    Assert.AreEqual(_contact.Email, contact.Email);
  }
  
  [TestMethod]
  public async Task CanRemoveContact()
  {
    var contactService = new ContactService(_context!);

    _context?.Contacts.Add(_contact);
    await _context?.SaveChangesAsync()!;
    var contact = await _context.Contacts.FindAsync(_contact.Email);
    Assert.IsNotNull(contact);
    Assert.AreEqual(1, _context.Contacts.Count());
    
    var removedContact = contactService.RemoveContact(_contact).Entity;
    await _context.SaveChangesAsync();
    Assert.IsNotNull(removedContact);
    Assert.AreEqual(_contact.Email, contact.Email);
    Assert.AreEqual(0, _context.Graphs.Count());
  }

  [TestMethod]
  public async Task TestContactExists()
  {
    var contactService = new ContactService(_context!);

    Assert.IsFalse(contactService.ContactExists(_contact.Email));

    contactService.AddContact(_contact);
    await contactService.SaveChangesAsync();

    Assert.IsTrue(contactService.ContactExists(_contact.Email));
  }
}