﻿using Microsoft.IdentityModel.Tokens;
using System;

namespace Neptune.NsPay.OpenIddict.Applications
{
    [Serializable]
    public class OpenIddictApplicationModel
    {
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the application type associated with the application.
        /// </summary>
        public virtual string ApplicationType { get; set; }

        /// <summary>
        /// Gets or sets the client identifier associated with the current application.
        /// </summary>
        public virtual string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret associated with the current application.
        /// Note: depending on the application manager used to create this instance,
        /// this property may be hashed or encrypted for security reasons.
        /// </summary>
        public virtual string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the client type associated with the application.
        /// </summary>
        public string ClientType { get; set; }

        /// <summary>
        /// Gets or sets the consent type associated with the current application.
        /// </summary>
        public virtual string ConsentType { get; set; }

        /// <summary>
        /// Gets or sets the display name associated with the current application.
        /// </summary>
        public virtual string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the JSON Web Key Set associated with the application.
        /// </summary>
        public virtual JsonWebKeySet JsonWebKeySet { get; set; }

        /// <summary>
        /// Gets or sets the localized display names
        /// associated with the current application,
        /// serialized as a JSON object.
        /// </summary>
        public virtual string DisplayNames { get; set; }

        /// <summary>
        /// Gets or sets the permissions associated with the
        /// current application, serialized as a JSON array.
        /// </summary>
        public virtual string Permissions { get; set; }

        /// <summary>
        /// Gets or sets the logout callback URLs associated with
        /// the current application, serialized as a JSON array.
        /// </summary>
        public virtual string PostLogoutRedirectUris { get; set; }

        /// <summary>
        /// Gets or sets the additional properties serialized as a JSON object,
        /// or <c>null</c> if no bag was associated with the current application.
        /// </summary>
        public virtual string Properties { get; set; }

        /// <summary>
        /// Gets or sets the callback URLs associated with the
        /// current application, serialized as a JSON array.
        /// </summary>
        public virtual string RedirectUris { get; set; }

        /// <summary>
        /// Gets or sets the requirements associated with the
        /// current application, serialized as a JSON array.
        /// </summary>
        public virtual string Requirements { get; set; }

        /// <summary>
        /// Gets the settings associated with the application.
        /// </summary>
        public virtual string Settings { get; set; }

        /// <summary>
        /// URI to further information about client.
        /// </summary>
        public string ClientUri { get; set; }

        /// <summary>
        /// URI to client logo.
        /// </summary>
        public string LogoUri { get; set; }
    }
}