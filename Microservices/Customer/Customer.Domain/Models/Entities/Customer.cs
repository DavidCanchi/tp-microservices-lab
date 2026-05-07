namespace Customer.Domain.Models.Entities;

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Email { get; set; }
    public required string Address { get; set; }
    public DateTime RegistrationDate { get; set; }

    public Customer() { }

    public Customer(string name, string email, string address, DateTime registrationDate)
    {
        Name = name;
        Email = email;
        Address = address;
        RegistrationDate = registrationDate;
    }
}

