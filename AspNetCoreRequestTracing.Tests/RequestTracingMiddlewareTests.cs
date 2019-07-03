using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using AspNetCoreRequestTracing.Tests.Server;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;

namespace AspNetCoreRequestTracing.Tests
{
    [Collection(ServerCollection.Name)]
    public class RequestTracingMiddlewareTests
    {
        private readonly ServerFixture _fixture;

        public RequestTracingMiddlewareTests(ServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task ItShouldLogGetJsonRequests()
        {
            _fixture.TestLogger.Messages.Clear();
            var client = new HttpClient
            {
                BaseAddress = _fixture.ServerUri,
            };

            var response = await client.GetAsync(ApiController.JsonUri);
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Name.Should().Be(SampleModel.DefaultName);

            var requestTrace = _fixture.TestLogger.Messages.FirstOrDefault(x => x.EventId.Name.Equals("RequestTrace", StringComparison.Ordinal));
            requestTrace.Should().NotBeNull();
            requestTrace.StateDictionary["RequestMethod"].Should().Be("GET");
            ((string)requestTrace.StateDictionary["RequestUri"]).Should().EndWith(ApiController.JsonUri);
            ((string)requestTrace.StateDictionary["RequestProtocol"]).Should().NotBeNullOrEmpty();
            ((string)requestTrace.StateDictionary["RequestHeaders"]).Should().NotBeNullOrEmpty();

            var responseTrace = _fixture.TestLogger.Messages.FirstOrDefault(x => x.EventId.Name.Equals("ResponseTrace", StringComparison.Ordinal));
            responseTrace.Should().NotBeNull();
            ((int)responseTrace.StateDictionary["ResponseStatusCode"]).Should().Be(200);
            ((string)responseTrace.StateDictionary["ResponseHeaders"]).Should().NotBeNullOrEmpty();
            ((string)responseTrace.StateDictionary["ResponseBody"]).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ItShouldLogPostJsonRequests()
        {
            _fixture.TestLogger.Messages.Clear();
            var client = new HttpClient
            {
                BaseAddress = _fixture.ServerUri,
            };

            var model = new SampleModel { Name = "foo" };
            var response = await client.PostAsJsonAsync(ApiController.JsonUri, model);
            var result = await response.Content.ReadAsAsync<SampleModel>();
            result.Should().BeEquivalentTo(model);

            var requestTrace = _fixture.TestLogger.Messages.FirstOrDefault(x => x.EventId.Name.Equals("RequestTrace", StringComparison.Ordinal));
            requestTrace.Should().NotBeNull();
            requestTrace.StateDictionary["RequestMethod"].Should().Be("POST");
            ((string)requestTrace.StateDictionary["RequestUri"]).Should().EndWith(ApiController.JsonUri);
            ((string)requestTrace.StateDictionary["RequestProtocol"]).Should().NotBeNullOrEmpty();
            ((string)requestTrace.StateDictionary["RequestHeaders"]).Should().NotBeNullOrEmpty();
            ((string)requestTrace.StateDictionary["RequestBody"]).Should().NotBeNullOrEmpty();

            var responseTrace = _fixture.TestLogger.Messages.FirstOrDefault(x => x.EventId.Name.Equals("ResponseTrace", StringComparison.Ordinal));
            responseTrace.Should().NotBeNull();
            ((int)responseTrace.StateDictionary["ResponseStatusCode"]).Should().Be(200);
            ((string)responseTrace.StateDictionary["ResponseHeaders"]).Should().NotBeNullOrEmpty();
            ((string)responseTrace.StateDictionary["ResponseBody"]).Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task ItShouldLogGetBinary()
        {
            _fixture.TestLogger.Messages.Clear();
            var client = new HttpClient
            {
                BaseAddress = _fixture.ServerUri,
            };

            var response = await client.GetAsync(ApiController.BinaryUri);
            var result = await response.Content.ReadAsByteArrayAsync();
            result.Should().BeEquivalentTo(await File.ReadAllBytesAsync(typeof(ApiController).Assembly.Location));

            var requestTrace = _fixture.TestLogger.Messages.FirstOrDefault(x => x.EventId.Name.Equals("RequestTrace", StringComparison.Ordinal));
            requestTrace.Should().NotBeNull();
            requestTrace.StateDictionary["RequestMethod"].Should().Be("GET");
            ((string)requestTrace.StateDictionary["RequestUri"]).Should().EndWith(ApiController.BinaryUri);
            ((string)requestTrace.StateDictionary["RequestProtocol"]).Should().NotBeNullOrEmpty();

            var responseTrace = _fixture.TestLogger.Messages.FirstOrDefault(x => x.EventId.Name.Equals("ResponseTrace", StringComparison.Ordinal));
            responseTrace.Should().NotBeNull();
            ((int)responseTrace.StateDictionary["ResponseStatusCode"]).Should().Be(200);
            ((string)responseTrace.StateDictionary["ResponseHeaders"]).Should().NotBeNullOrEmpty();
            ((string)responseTrace.StateDictionary["ResponseBody"]).Should().NotBeNullOrEmpty();
        }
    }
}
