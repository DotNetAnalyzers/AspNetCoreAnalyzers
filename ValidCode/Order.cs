namespace ValidCode
{
    using System.Collections.Generic;

    public class Order
    {
        public int Id { get; set; }

        public IEnumerable<OrderItem>? Items { get; set; }
    }
}
