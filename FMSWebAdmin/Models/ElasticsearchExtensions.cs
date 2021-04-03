/*using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModelsFeedbackSystem.Models;
using Nest;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FMSWebAdmin.Models
{
    public static class ElasticsearchExtensions
    {
        public static void AddElasticsearch(this IServiceCollection services, IConfiguration configuration)
        {
            var url = configuration["elasticsearch:http://localhost:9200"];
            string defaultIndex = "responseoffeedback";

            var settings = new ConnectionSettings(new Uri("http://localhost:9200"))
                .DefaultIndex("responseoffeedback")
                .DefaultMappingFor<Response>(m => m.IndexName("responseoffeedback"));

            //AddDefaultMappings(settings);

            var client = new ElasticClient(settings);
           *//* var responseSetDefaultIndex = client.Search<Response>();*//*


            services.AddSingleton(client);

           // CreateIndex(client, defaultIndex);
        }

       *//* private static void AddDefaultMappings(ConnectionSettings settings)
        {
            IndexSettings
                  DefaultMappingFor<Response>(m => m
                  .Ignore(p => p.Price)
                  .Ignore(p => p.Quantity)
                  .Ignore(p => p.Rating)
              );
        }
*//*
        private static void CreateIndex(IElasticClient client, string indexName)
        {
            var createIndexResponse = client.Indices.Create(indexName,
                index => index.Map<Response>(x => x.AutoMap())
            );
        }
    }
}
*/