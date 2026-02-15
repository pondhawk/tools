namespace Pondhawk.Rules.Tests;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
    public string State { get; set; }
    public bool IsActive { get; set; }
    public decimal Salary { get; set; }
    public DateTime BirthDate { get; set; }
    public string Status { get; set; }
    public List<Address> Addresses { get; set; } = [];

    public override string ToString() => $"Person({Name})";
}

public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }

    public override string ToString() => $"Address({Street})";
}

public class Order
{
    public int OrderId { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; } = [];

    public override string ToString() => $"Order({OrderId})";
}

public class OrderItem
{
    public string Product { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public override string ToString() => $"OrderItem({Product})";
}

public class Account
{
    public string AccountId { get; set; }
    public decimal Balance { get; set; }
    public string Type { get; set; }

    public override string ToString() => $"Account({AccountId})";
}

public class Policy
{
    public string PolicyId { get; set; }
    public string Category { get; set; }
    public int Priority { get; set; }

    public override string ToString() => $"Policy({PolicyId})";
}
