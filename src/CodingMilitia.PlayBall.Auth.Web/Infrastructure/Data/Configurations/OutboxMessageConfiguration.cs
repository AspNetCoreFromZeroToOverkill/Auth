using CodingMilitia.PlayBall.Auth.Web.Data;
using CodingMilitia.PlayBall.Auth.Web.Data.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;

namespace CodingMilitia.PlayBall.Auth.Web.Infrastructure.Data.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                
            };
            
            builder
                .HasKey(e => e.Id);

            builder
                .Property(e => e.Id)
                .UseIdentityAlwaysColumn();

            builder
                .Property(e => e.Event)
                // using json instead of jsonb, as Newtonsoft.Json expects the $type property to be the first, but jsonb might reorder properties
                .HasColumnType("json")
                // Npgsql supports JSON out of the box, but doesn't handle hierarchies, so just using Newtonsoft to avoid more work 
                .HasConversion(
                    e => JsonConvert.SerializeObject(e, settings),
                    e => JsonConvert.DeserializeObject<BaseAuthEvent>(e, settings));
        }
    }
}