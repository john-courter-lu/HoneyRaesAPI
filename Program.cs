using Microsoft.AspNetCore.Http.Json;
using System.Text.Json.Serialization;
using HoneyRaesAPI.Models;

List<Customer> customers = new List<Customer>
    {
        new Customer { Id = 1, Name = "John Doe", Address = "123 Main St" },
        new Customer { Id = 2, Name = "Jane Smith", Address = "456 Elm St" },
        new Customer { Id = 3, Name = "Robert Johnson", Address = "789 Oak St" }
    };

List<Employee> employees = new List<Employee>
    {
        new Employee { Id = 1, Name = "Alice Brown", Specialty = "Plumber" },
        new Employee { Id = 2, Name = "Bob Green", Specialty = "Electrician" }
    };

List<ServiceTicket> serviceTickets = new List<ServiceTicket>
    {
        new ServiceTicket { Id = 1, CustomerId = 1, EmployeeId = 1, Description = "Leaky faucet repair", Emergency = false, DateCompleted = DateTime.Now.AddDays(-1) },
        new ServiceTicket { Id = 2, CustomerId = 2, EmployeeId = 1, Description = "Power outage troubleshooting", Emergency = true, DateCompleted = DateTime.Now.AddDays(-2) },
        new ServiceTicket { Id = 3, CustomerId = 3, EmployeeId = 2, Description = "Wiring installation", Emergency = false, DateCompleted = DateTime.Now.AddDays(-3) },
        new ServiceTicket { Id = 4, CustomerId = 1, EmployeeId = 2, Description = "Circuit breaker replacement", Emergency = true, DateCompleted = DateTime.Now.AddDays(-4) },
        new ServiceTicket { Id = 5, CustomerId = 2, EmployeeId = 1, Description = "Sink drain cleaning", Emergency = false },
        new ServiceTicket { Id = 6, CustomerId = 3, EmployeeId = 2, Description = "Light fixture installation", Emergency = false },
        new ServiceTicket { Id = 7, CustomerId = 1, EmployeeId = 1, Description = "Toilet repair", Emergency = true },
        new ServiceTicket { Id = 8, CustomerId = 2, Description = "Outlet replacement", Emergency = false }
    };

var builder = WebApplication.CreateBuilder(args);

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//上面的code是设置一个app来be served as a web API

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

//下面是重点 
//创建了一个endpoint =  a route (a URL to make a request) + a handler / The lambda function
// MapGet Get request

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/serviceTickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(e => e.Id == id);

    if (serviceTicket == null)
    {
        return Results.NotFound();
    }

    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(e => e.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);

});

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employees/{id}", (int id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == id);

    if (employee == null)
    {
        return Results.NotFound();
    }

    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == id).ToList();
    return Results.Ok(employee);

});

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{id}", (int id) =>
{
    Customer customer = customers.FirstOrDefault(e => e.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == id).ToList();
    return Results.Ok(customer);

});

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Count > 0 ? serviceTickets.Max(st => st.Id) + 1 : 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/serviceTickets/{id}", (int id) =>
{

    ServiceTicket ticketToRemove = serviceTickets.FirstOrDefault(ticket => ticket.Id == id);

    if (ticketToRemove != null)
    {
        serviceTickets.Remove(ticketToRemove);
    }

});

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket updatedServiceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != updatedServiceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = updatedServiceTicket;
    return Results.Ok();
});

// 教材中用的是MapPost
// 但Greg说, 若已经有id了,就用Put, 没有才用Post,
// 所以把complete从MapPost改为了MapPut
// 但后来又学到MapPost是create or update, MapPut是edit
app.MapPost("/servicetickets/{id}/complete", (int id) =>
{

ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
ticketToComplete.DateCompleted = DateTime.Today;

});

app.Run();
// 这里是run this app


// 这和定义class一样