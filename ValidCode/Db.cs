namespace ValidCode;

using Microsoft.EntityFrameworkCore;

public class Db : DbContext
{
    public DbSet<Order> Orders => this.Set<Order>();
}
