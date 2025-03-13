using Microsoft.AspNetCore.Mvc;
using RepositoryLayer.Entity;
using RepositoryLayer.Context;
using System.Security.Cryptography.X509Certificates;

namespace AddressBookApplication.Controllers;

[ApiController]
[Route("api/addressbook")]
public class AddressBookController : ControllerBase

{
    private readonly AddressBookContext _context;
    public AddressBookController(AddressBookContext context)
    {
        _context = context;
    }
    ///<summary> Fetch all the contacts from the address book </summary>
    /// <returns> List of contacts </returns>
    [HttpGet]
    public ActionResult<IEnumerable<AddressBookEntity>> GetAllContacts()
    {
        try
        {
            var contacts = _context.AddressBooks.ToList();
            if (!contacts.Any())
            {
                return NotFound("No contacts found");
            }
            return Ok(contacts);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Server Error");

        }
    }

        ///<summary> Get a Contact by ID </summary>
        /// <param name="id"> ID of the contact </param>
        /// <returns> Contact with the given ID </returns>
        [HttpGet("{id}")]
        public ActionResult<AddressBookEntity> GetContactById(int id)
        {
            try
            {
                var contact = _context.AddressBooks.Find(id);
                if (contact == null)
                {
                    return NotFound("Contact not found");
                }
                return Ok(contact);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

    ///<summary> Add a new contact to the address book </summary>
    /// <param name="contact"> Contact to be added </param>
    /// <returns> The added contact </returns>
    [HttpPost]
    public ActionResult<AddressBookEntity> AddContact(AddressBookEntity contact)
    {
        try
        {
            _context.AddressBooks.Add(contact);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetContactById), new { id = contact.Id }, contact);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
    /// <summary>
    /// Update an existing contact
    /// </summary>
    /// <param name="id">Id of the contact</param>
    /// <param name="updatedContact">Updated contact details</param>
    /// <returns>Updated Contact</returns>
    [HttpPut("{id}")]
    public ActionResult<AddressBookEntity> UpdateContact(int id, AddressBookEntity updatedContact)
    {
        try
        {
            var existingContact = _context.AddressBooks.Find(id);
            if (existingContact == null)
            {
                return NotFound($"Contact with ID {id} not found.");
            }

            existingContact.Name = updatedContact.Name;
            existingContact.Email = updatedContact.Email;
            existingContact.Phone = updatedContact.Phone;
            existingContact.Address = updatedContact.Address;

            _context.SaveChanges();
            return Ok(existingContact);
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }


    /// <summary>
    /// Delete a contact by ID
    /// </summary>
    /// <param name="id">ID of the contact</param>
    /// <returns>Deletion status</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteContact(int id)
    {
        try
        {
            var contact = _context.AddressBooks.Find(id);
            if (contact == null)
            {
                return NotFound($"Contact with ID {id} not found.");
            }

            _context.AddressBooks.Remove(contact);
            _context.SaveChanges();
            return Ok("Contact deleted successfully.");
        }
        catch (Exception)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }


}

