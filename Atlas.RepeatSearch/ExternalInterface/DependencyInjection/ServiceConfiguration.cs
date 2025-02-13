﻿using Atlas.Common.ApplicationInsights;
using Atlas.Common.Notifications;
using Atlas.Common.Utils.Extensions;
using Atlas.HlaMetadataDictionary.ExternalInterface.Settings;
using Atlas.MatchingAlgorithm.DependencyInjection;
using Atlas.MatchingAlgorithm.Settings;
using Atlas.MatchingAlgorithm.Settings.Azure;
using Atlas.MultipleAlleleCodeDictionary.Settings;
using Atlas.RepeatSearch.Clients;
using Atlas.RepeatSearch.Services.Search;
using Atlas.RepeatSearch.Settings.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using System;
using Atlas.Common.AzureStorage.Blob;
using Atlas.DonorImport.ExternalInterface.DependencyInjection;
using Atlas.RepeatSearch.Data.Context;
using Atlas.RepeatSearch.Data.Repositories;
using Atlas.RepeatSearch.Services.ResultSetTracking;
using ConnectionStrings = Atlas.RepeatSearch.Data.Settings.ConnectionStrings;
using Atlas.MatchingAlgorithm.ApplicationInsights.ContextAwareLogging;

namespace Atlas.RepeatSearch.ExternalInterface.DependencyInjection
{
    public static class ServiceConfiguration
    {
        public static void RegisterRepeatSearch(
            this IServiceCollection services,
            Func<IServiceProvider, ApplicationInsightsSettings> fetchApplicationInsightsSettings,
            Func<IServiceProvider, RepeatSearch.Settings.Azure.AzureStorageSettings> fetchAzureStorageSettings,
            Func<IServiceProvider, HlaMetadataDictionarySettings> fetchHlaMetadataDictionarySettings,
            Func<IServiceProvider, MacDictionarySettings> fetchMacDictionarySettings,
            Func<IServiceProvider, MatchingConfigurationSettings> fetchMatchingConfigurationSettings,
            Func<IServiceProvider, MessagingServiceBusSettings> fetchMessagingServiceBusSettings,
            Func<IServiceProvider, NotificationsServiceBusSettings> fetchNotificationsServiceBusSettings,
            Func<IServiceProvider, string> fetchRepeatSqlConnectionString,
            Func<IServiceProvider, string> fetchPersistentSqlConnectionString,
            Func<IServiceProvider, string> fetchTransientASqlConnectionString,
            Func<IServiceProvider, string> fetchTransientBSqlConnectionString,
            Func<IServiceProvider, string> fetchDonorSqlConnectionString)
        {
            services.RegisterSettings(
                fetchApplicationInsightsSettings,
                fetchAzureStorageSettings,
                fetchMessagingServiceBusSettings,
                fetchRepeatSqlConnectionString);
            
            services.RegisterServices(fetchRepeatSqlConnectionString, fetchAzureStorageSettings);

            services.RegisterSearch(
                fetchApplicationInsightsSettings,
                // Matching algorithm doesn't require an azure storage connection, as results upload is handled by repeat search.
                _ => new AzureStorageSettings(),
                fetchHlaMetadataDictionarySettings,
                fetchMacDictionarySettings,
                // Matching algorithm doesn't require a service bus setting as results notifications are handled by repeat search.
                _ => new MatchingAlgorithm.Settings.ServiceBus.MessagingServiceBusSettings(),
                fetchNotificationsServiceBusSettings,
                fetchMatchingConfigurationSettings,
                fetchPersistentSqlConnectionString,
                fetchTransientASqlConnectionString,
                fetchTransientBSqlConnectionString,
                fetchDonorSqlConnectionString);
            
            services.RegisterDonorReader(fetchDonorSqlConnectionString);
        }

        private static void RegisterSettings(
            this IServiceCollection services,
            Func<IServiceProvider, ApplicationInsightsSettings> fetchApplicationInsightsSettings,
            Func<IServiceProvider, RepeatSearch.Settings.Azure.AzureStorageSettings> fetchAzureStorageSettings,
            Func<IServiceProvider, MessagingServiceBusSettings> fetchMessagingServiceBusSettings,
            Func<IServiceProvider, string> fetchRepeatSqlConnectionString)
        {
            services.MakeSettingsAvailableForUse(fetchApplicationInsightsSettings);
            services.MakeSettingsAvailableForUse(fetchAzureStorageSettings);
            services.MakeSettingsAvailableForUse(fetchMessagingServiceBusSettings);

            services.AddSingleton(sp => new ConnectionStrings {RepeatSearchSqlConnectionString = fetchRepeatSqlConnectionString(sp)});
        }

        private static void RegisterServices(
            this IServiceCollection services, 
            Func<IServiceProvider, string> fetchRepeatSqlConnectionString, 
            Func<IServiceProvider, Settings.Azure.AzureStorageSettings> fetchAzureStorageSettings)
        {
            services.AddScoped<IOriginalSearchResultSetTracker, OriginalSearchResultSetTracker>();
            services.AddSingleton<IBlobDownloader>(sp =>
            {
                var storageSettings = sp.GetService<RepeatSearch.Settings.Azure.AzureStorageSettings>();
                var logger = sp.GetService<ILogger>();
                return new BlobDownloader(storageSettings.ConnectionString, logger);
            });

            services.AddScoped(sp => new ContextFactory().Create(fetchRepeatSqlConnectionString(sp)));
            services.AddScoped<ICanonicalResultSetRepository, CanonicalResultSetRepository>();
            services.AddScoped<IRepeatSearchHistoryRepository, RepeatSearchHistoryRepository>();

            services.AddScoped<IRepeatSearchDispatcher, RepeatSearchDispatcher>();
            services.AddScoped<IRepeatSearchRunner, RepeatSearchRunner>();
            services.AddScoped<IRepeatSearchServiceBusClient, RepeatSearchServiceBusClient>();
            services.AddScoped<ISearchResultsBlobStorageClient, SearchResultsBlobStorageClient>(sp =>
            {
                var settings = fetchAzureStorageSettings(sp);
                var logger = sp.GetService<IMatchingAlgorithmSearchLogger>();
                return new SearchResultsBlobStorageClient(settings.ConnectionString, logger);
            });

            services.AddScoped<IRepeatSearchValidator, RepeatSearchValidator>();
            
            services.AddScoped<IRepeatSearchDifferentialCalculator, RepeatSearchDifferentialCalculator>();
        }
    }
}