﻿using System;
using Umbraco.Core.Composing;
using Current = Umbraco.Web.Composing.Current;
using Umbraco.Web.Actions;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Tour;

// the namespace here is intentional -  although defined in Umbraco.Web assembly,
// this class should be visible when using Umbraco.Core.Components, alongside
// Umbraco.Core's own CompositionExtensions class

// ReSharper disable once CheckNamespace
namespace Umbraco.Core.Components
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class WebCompositionExtensions
    {
        #region Collection Builders

        /// <summary>
        /// Gets the actions collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        internal static ActionCollectionBuilder Actions(this Composition composition)
            => composition.WithCollectionBuilder<ActionCollectionBuilder>();

        /// <summary>
        /// Gets the content apps collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static ContentAppFactoryCollectionBuilder ContentApps(this Composition composition)
            => composition.WithCollectionBuilder<ContentAppFactoryCollectionBuilder>();

        /// <summary>
        /// Gets the content finders collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static ContentFinderCollectionBuilder ContentFinders(this Composition composition)
            => composition.WithCollectionBuilder<ContentFinderCollectionBuilder>();

        /// <summary>
        /// Gets the editor validators collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        internal static EditorValidatorCollectionBuilder EditorValidators(this Composition composition)
            => composition.WithCollectionBuilder<EditorValidatorCollectionBuilder>();

        /// <summary>
        /// Gets the filtered controller factories collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static FilteredControllerFactoryCollectionBuilder FilderedControllerFactory(this Composition composition)
            => composition.WithCollectionBuilder<FilteredControllerFactoryCollectionBuilder>();

        /// <summary>
        /// Gets the health checks collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        public static HealthCheckCollectionBuilder HealthChecks(this Composition composition)
            => composition.WithCollectionBuilder<HealthCheckCollectionBuilder>();

        /// <summary>
        /// Gets the TourFilters collection builder.
        /// </summary>
        public static TourFilterCollectionBuilder TourFilters(this Composition composition)
            => composition.WithCollectionBuilder<TourFilterCollectionBuilder>();

        /// <summary>
        /// Gets the url providers collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        internal static UrlProviderCollectionBuilder UrlProviders(this Composition composition)
            => composition.WithCollectionBuilder<UrlProviderCollectionBuilder>();

        #endregion

        #region Uniques

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <typeparam name="T">The type of the content last chance finder.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetContentLastChanceFinder<T>(this Composition composition)
            where T : IContentLastChanceFinder
        {
            composition.RegisterUnique<IContentLastChanceFinder, T>();
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a last chance finder.</param>
        public static void SetContentLastChanceFinder(this Composition composition, Func<IFactory, IContentLastChanceFinder> factory)
        {
            composition.RegisterUnique(factory);
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="finder">A last chance finder.</param>
        public static void SetContentLastChanceFinder(this Composition composition, IContentLastChanceFinder finder)
        {
            composition.RegisterUnique(_ => finder);
        }

        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <typeparam name="T">The type of the published snapshot service.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetPublishedSnapshotService<T>(this Composition composition)
            where T : IPublishedSnapshotService
        {
            composition.RegisterUnique<IPublishedSnapshotService, T>();
        }

        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a published snapshot service.</param>
        public static void SetPublishedSnapshotService(this Composition composition, Func<IFactory, IPublishedSnapshotService> factory)
        {
            composition.RegisterUnique(factory);
        }

        /// <summary>
        /// Sets the published snapshot service.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="service">A published snapshot service.</param>
        public static void SetPublishedSnapshotService(this Composition composition, IPublishedSnapshotService service)
        {
            composition.RegisterUnique(_ => service);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <typeparam name="T">The type of the site domain helper.</typeparam>
        /// <param name="composition"></param>
        public static void SetSiteDomainHelper<T>(this Composition composition)
            where T : ISiteDomainHelper
        {
            composition.RegisterUnique<ISiteDomainHelper, T>();
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a helper.</param>
        public static void SetSiteDomainHelper(this Composition composition, Func<IFactory, ISiteDomainHelper> factory)
        {
            composition.RegisterUnique(factory);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="helper">A helper.</param>
        public static void SetSiteDomainHelper(this Composition composition, ISiteDomainHelper helper)
        {
            composition.RegisterUnique(_ => helper);
        }

        /// <summary>
        /// Sets the default controller for rendering template views.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="composition">The composition.</param>
        /// <remarks>The controller type is registered to the container by the composition.</remarks>
        public static void SetDefaultRenderMvcController<TController>(this Composition composition)
            => composition.SetDefaultRenderMvcController(typeof(TController));

        /// <summary>
        /// Sets the default controller for rendering template views.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <remarks>The controller type is registered to the container by the composition.</remarks>
        public static void SetDefaultRenderMvcController(this Composition composition, Type controllerType)
        {
            composition.OnCreatingFactory["Umbraco.Core.DefaultRenderMvcController"] = () =>
            {
                // no need to register: all IRenderMvcController are registered
                //composition.Register(controllerType, Lifetime.Request);
                Current.DefaultRenderMvcControllerType = controllerType;
            };
        }

        #endregion
    }
}
