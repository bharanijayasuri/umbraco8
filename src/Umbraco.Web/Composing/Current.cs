﻿using System;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Events;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Persistence;
using Umbraco.Core.Composing;
using Umbraco.Core.Configuration;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Core.Strings;
using Umbraco.Core.Sync;
using Umbraco.Core._Legacy.PackageActions;
using Umbraco.Web.Actions;
using Umbraco.Web.Cache;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.WebApi;

using CoreCurrent = Umbraco.Core.Composing.Current;

namespace Umbraco.Web.Composing
{
    // see notes in Umbraco.Core.Composing.Current.
    public static class Current
    {
        private static readonly object Locker = new object();

        private static IUmbracoContextAccessor _umbracoContextAccessor;

        static Current()
        {
            CoreCurrent.Resetted += (sender, args) =>
            {
                if (_umbracoContextAccessor != null)
                    ClearUmbracoContext();
                _umbracoContextAccessor = null;
            };
        }

        // for UNIT TESTS exclusively!
        internal static void Reset()
        {
            CoreCurrent.Reset();
        }

        /// <summary>
        /// Gets the factory.
        /// </summary>
        public static IFactory Factory
            => CoreCurrent.Factory;

        #region Temp & Special

        // fixme - have to keep this until tests are refactored
        // but then, it should all be managed properly in the container
        public static IUmbracoContextAccessor UmbracoContextAccessor
        {
            get
            {
                if (_umbracoContextAccessor != null) return _umbracoContextAccessor;
                return _umbracoContextAccessor = Factory.GetInstance<IUmbracoContextAccessor>();
            }
            set => _umbracoContextAccessor = value; // for tests
        }

        // clears the "current" umbraco context
        // at the moment the "current" umbraco context can end up being disposed and should get cleared
        // in the accessor - this should be done differently but for the time being we have to support it
        public static void ClearUmbracoContext()
        {
            lock (Locker)
            {
                UmbracoContextAccessor.UmbracoContext?.Dispose(); // dispose the one that is being cleared, if any
                UmbracoContextAccessor.UmbracoContext = null;
            }
        }

        #endregion

        #region Web Getters

        public static UmbracoContext UmbracoContext
            => UmbracoContextAccessor.UmbracoContext;

        public static DistributedCache DistributedCache
            => Factory.GetInstance<DistributedCache>();

        public static IPublishedSnapshot PublishedSnapshot
            => Factory.GetInstance<IPublishedSnapshotAccessor>().PublishedSnapshot;

        public static EventMessages EventMessages
            => Factory.GetInstance<IEventMessagesFactory>().GetOrDefault();

        public static UrlProviderCollection UrlProviders
            => Factory.GetInstance<UrlProviderCollection>();

        public static HealthCheckCollectionBuilder HealthCheckCollectionBuilder
            => Factory.GetInstance<HealthCheckCollectionBuilder>();

        internal static ActionCollectionBuilder ActionCollectionBuilder
            => Factory.GetInstance<ActionCollectionBuilder>();

        public static ActionCollection Actions
            => Factory.GetInstance<ActionCollection>();

        public static ContentFinderCollection ContentFinders
            => Factory.GetInstance<ContentFinderCollection>();

        public static IContentLastChanceFinder LastChanceContentFinder
            => Factory.GetInstance<IContentLastChanceFinder>();

        internal static EditorValidatorCollection EditorValidators
            => Factory.GetInstance<EditorValidatorCollection>();

        internal static UmbracoApiControllerTypeCollection UmbracoApiControllerTypes
            => Factory.GetInstance<UmbracoApiControllerTypeCollection>();

        internal static SurfaceControllerTypeCollection SurfaceControllerTypes
            => Factory.GetInstance<SurfaceControllerTypeCollection>();

        public static FilteredControllerFactoryCollection FilteredControllerFactories
            => Factory.GetInstance<FilteredControllerFactoryCollection>();

        internal static IPublishedSnapshotService PublishedSnapshotService
            => Factory.GetInstance<IPublishedSnapshotService>();

        #endregion

        #region Web Constants

        // these are different - not 'resolving' anything, and nothing that could be managed
        // by the container - just registering some sort of application-wide constants or
        // settings - but they fit in Current nicely too

        private static Type _defaultRenderMvcControllerType;

        // internal - can only be accessed through Composition at compose time
        internal static Type DefaultRenderMvcControllerType
        {
            get => _defaultRenderMvcControllerType;
            set
            {
                if (value.Implements<IRenderController>() == false)
                    throw new ArgumentException($"Type {value.FullName} does not implement {typeof (IRenderController).FullName}.", nameof(value));
                _defaultRenderMvcControllerType = value;
            }
        }

        #endregion

        #region Web Actions

        public static void RestartAppPool()
        {
            // see notes in overload

            var httpContext = HttpContext.Current;
            if (httpContext != null)
            {
                httpContext.Application.Add("AppPoolRestarting", true);
                httpContext.User = null;
            }
            Thread.CurrentPrincipal = null;
            HttpRuntime.UnloadAppDomain();
        }

        public static void RestartAppPool(HttpContextBase httpContext)
        {
            // we're going to put an application wide flag to show that the application is about to restart.
            // we're doing this because if there is a script checking if the app pool is fully restarted, then
            // it can check if this flag exists...  if it does it means the app pool isn't restarted yet.
            httpContext.Application.Add("AppPoolRestarting", true);

            // unload app domain - we must null out all identities otherwise we get serialization errors
            // http://www.zpqrtbnk.net/posts/custom-iidentity-serialization-issue
            httpContext.User = null;
            if (HttpContext.Current != null)
                HttpContext.Current.User = null;
            Thread.CurrentPrincipal = null;

            HttpRuntime.UnloadAppDomain();
        }

        #endregion

        #region Core Getters

        // proxy Core for convenience

        public static IRuntimeState RuntimeState => CoreCurrent.RuntimeState;

        public static TypeLoader TypeLoader => CoreCurrent.TypeLoader;

        public static Configs Configs => CoreCurrent.Configs;

        public static UrlSegmentProviderCollection UrlSegmentProviders => CoreCurrent.UrlSegmentProviders;

        public static CacheRefresherCollection CacheRefreshers => CoreCurrent.CacheRefreshers;

        public static DataEditorCollection DataEditors => CoreCurrent.DataEditors;

        public static PropertyEditorCollection PropertyEditors => CoreCurrent.PropertyEditors;

        public static ParameterEditorCollection ParameterEditors => CoreCurrent.ParameterEditors;

        internal static ManifestValueValidatorCollection ManifestValidators => CoreCurrent.ManifestValidators;

        internal static PackageActionCollection PackageActions => CoreCurrent.PackageActions;

        internal static PropertyValueConverterCollection PropertyValueConverters => CoreCurrent.PropertyValueConverters;

        internal static IPublishedModelFactory PublishedModelFactory => CoreCurrent.PublishedModelFactory;

        public static IServerMessenger ServerMessenger => CoreCurrent.ServerMessenger;

        public static IServerRegistrar ServerRegistrar => CoreCurrent.ServerRegistrar;

        public static ICultureDictionaryFactory CultureDictionaryFactory => CoreCurrent.CultureDictionaryFactory;

        public static IShortStringHelper ShortStringHelper => CoreCurrent.ShortStringHelper;

        public static ILogger Logger => CoreCurrent.Logger;

        public static IProfiler Profiler => CoreCurrent.Profiler;

        public static IProfilingLogger ProfilingLogger => CoreCurrent.ProfilingLogger;

        public static CacheHelper ApplicationCache => CoreCurrent.ApplicationCache;

        public static ServiceContext Services => CoreCurrent.Services;

        public static IScopeProvider ScopeProvider => CoreCurrent.ScopeProvider;

        public static IFileSystems FileSystems => CoreCurrent.FileSystems;

        public static ISqlContext SqlContext=> CoreCurrent.SqlContext;

        public static IPublishedContentTypeFactory PublishedContentTypeFactory => CoreCurrent.PublishedContentTypeFactory;

        public static IPublishedValueFallback PublishedValueFallback => CoreCurrent.PublishedValueFallback;

        public static IVariationContextAccessor VariationContextAccessor => CoreCurrent.VariationContextAccessor;

        #endregion
    }
}
