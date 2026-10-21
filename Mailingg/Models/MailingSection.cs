using System.Collections.Generic;

public class MailingSection
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Product> Products { get; set; } = new();
    public List<Customer> Customers { get; set; } = new();
}