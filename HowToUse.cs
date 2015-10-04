namespace ConsoleValidatio
{
    class Program
    {
        static void Main(string[] args)
        {
            var customer = new Customer { Id = 0, LegalIdentifier = "S12" };
            var customerValidationFor = new CustomerValidator { StopToContinue = false };
            var result = customerValidationFor.Validate(customer);

            if (!result.IsValid)
            {
                Console.WriteLine(result.ErrorMessage);
                Console.ReadLine();
                return;
            };

            Console.WriteLine("Validate successfully");
            Console.ReadLine();
        }
    }

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LegalIdentifier { get; set; }
    }

    public class CustomerValidator : BaseValidationFor<Customer>
    {
        public CustomerValidator()
        {
            ValidationFor()
                .ShouldBe(item => item.Id > 0, item => string.Format("Customer id {0} should not valid", item.Id))
                .ShouldBe(item => item.LegalIdentifier.NotNull(), "it should not be null")
                .ShouldBe(item => item.LegalIdentifier.NotEmpty())
                .ShouldBe(item => item.Name.NotEmpty(), item => "Customer name should not be empty.")
                .ShouldBe(item => item.LegalIdentifier.IsNumber(), item => string.Format("{0} should be a number", item.LegalIdentifier))
                .ShouldBe(item => item.LegalIdentifier.Length(2), item => string.Format("{0} should be 2 characters long", item.LegalIdentifier));
        }
    }
