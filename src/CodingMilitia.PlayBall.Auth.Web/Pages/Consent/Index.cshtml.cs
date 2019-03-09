using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace CodingMilitia.PlayBall.Auth.Web.Pages.Consent
{
    //note: adapted from the quickstart examples, that use Controllers+Views
    //not thoroughly tested!
    
    public class IndexModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IResourceStore _resourceStore;
        private readonly IEventService _events;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IResourceStore resourceStore,
            IEventService events,
            ILogger<IndexModel> logger)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _events = events;
            _logger = logger;
        }
        
        [TempData] public string StatusMessage { get; set; }
        public string ReturnUrl { get; set; }
        public string ClientName { get; set; }
        public string ClientUrl { get; set; }
        public string ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; }

        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; } = Enumerable.Empty<ScopeViewModel>();
        public IEnumerable<ScopeViewModel> ResourceScopes { get; set; } = Enumerable.Empty<ScopeViewModel>();
        
        [BindProperty]
        public InputModel Input { get; set; }
        
        public class InputModel
        {
            public string Button { get; set; }
            public IEnumerable<string> ScopesConsented { get; set; }
            public bool RememberConsent { get; set; }
        }
        
        public class ConsentOptions
        {
            public static bool EnableOfflineAccess = true;
            public static string OfflineAccessDisplayName = "Offline Access";
            public static string OfflineAccessDescription = "Access to your applications and resources, even when you are offline";

            public static readonly string MustChooseOneErrorMessage = "You must pick at least one permission";
            public static readonly string InvalidSelectionErrorMessage = "Invalid selection";
        }
        
        public class ProcessConsentResult
        {
            public bool IsRedirect => RedirectUri != null;
            public string RedirectUri { get; set; }
            public string ClientId { get; set; }

            public bool ShowView { get; set; }

            public bool HasValidationError => ValidationError != null;
            public string ValidationError { get; set; }
        }

        /// <summary>
        /// Shows the consent screen
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<IActionResult> OnGetAsync(string returnUrl)
        {
            ReturnUrl = returnUrl;
            await BuildModelAsync(returnUrl);
            return Page();
        }

        /// <summary>
        /// Handles the consent screen postback
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl)
        {
            ReturnUrl = returnUrl;
            var result = await ProcessConsent();

            if (result.IsRedirect)
            {
//                if (await _clientStore.IsPkceClientAsync(result.ClientId))
//                {
//                    // if the client is PKCE then we assume it's native, so this change in how to
//                    // return the response is for better UX for the end user.
//                    return View("Redirect", new RedirectViewModel { RedirectUrl = result.RedirectUri });
//                }

                return Redirect(result.RedirectUri);
            }

            if (result.HasValidationError)
            {
                ModelState.AddModelError(string.Empty, result.ValidationError);
            }

            return Page();
        }

        /*****************************************/
        /* helper APIs for the Consent           */
        /*****************************************/
        private async Task<ProcessConsentResult> ProcessConsent()
        {
            var result = new ProcessConsentResult();

            // validate return url is still valid
            var request = await _interaction.GetAuthorizationContextAsync(ReturnUrl);
            if (request == null) return result;

            ConsentResponse grantedConsent = null;

            // user clicked 'no' - send back the standard 'access_denied' response
            if (Input?.Button == "no")
            {
                grantedConsent = ConsentResponse.Denied;

                // emit event
                await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested));
            }
            // user clicked 'yes' - validate the data
            else if (Input?.Button == "yes")
            {
                // if the user consented to some scope, build the response model
                if (Input.ScopesConsented != null && Input.ScopesConsented.Any())
                {
                    var scopes = Input.ScopesConsented;
                    if (ConsentOptions.EnableOfflineAccess == false)
                    {
                        scopes = scopes.Where(x => x != IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess);
                    }

                    grantedConsent = new ConsentResponse
                    {
                        RememberConsent = Input.RememberConsent,
                        ScopesConsented = scopes.ToArray()
                    };

                    // emit event
                    await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested, grantedConsent.ScopesConsented, grantedConsent.RememberConsent));
                }
                else
                {
                    result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
                }
            }
            else
            {
                result.ValidationError = ConsentOptions.InvalidSelectionErrorMessage;
            }

            if (grantedConsent != null)
            {
                // communicate outcome of consent back to identityserver
                await _interaction.GrantConsentAsync(request, grantedConsent);

                // indicate that's it ok to redirect back to authorization endpoint
                result.RedirectUri = ReturnUrl;
                result.ClientId = request.ClientId;
            }
            else
            {
                // we need to redisplay the consent UI
                await BuildModelAsync(ReturnUrl);
                result.ShowView = true;
            }

            return result;
        }

        private async Task<bool> BuildModelAsync(string returnUrl)
        {
            var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (request != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(request.ClientId);
                if (client != null)
                {
                    var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);
                    if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
                    {
                        FillConsentModel(returnUrl, request, client, resources);
                        return true;
                    }
                    else
                    {
                        _logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
                    }
                }
                else
                {
                    _logger.LogError("Invalid client id: {0}", request.ClientId);
                }
            }
            else
            {
                _logger.LogError("No consent request matching request: {0}", returnUrl);
            }

            StatusMessage = "An error occurred! (TODO: make message useful)";
            return false;
        }

        private void FillConsentModel(
            string returnUrl,
            AuthorizationRequest request,
            Client client, Resources resources)
        {
            Input = Input ?? new InputModel();
            
            Input.RememberConsent = Input?.RememberConsent ?? true;
            Input.ScopesConsented = Input?.ScopesConsented ?? Enumerable.Empty<string>();

            ClientName = client.ClientName ?? client.ClientId;
            ClientUrl = client.ClientUri;
            ClientLogoUrl = client.LogoUri;
            AllowRememberConsent = client.AllowRememberConsent;

            IdentityScopes = resources.IdentityResources.Select(x => CreateScopeViewModel(x, Input.ScopesConsented.Contains(x.Name))).ToArray();
            ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(x => CreateScopeViewModel(x, Input.ScopesConsented.Contains(x.Name))).ToArray();
           
            if (ConsentOptions.EnableOfflineAccess && resources.OfflineAccess)
            {
                ResourceScopes = ResourceScopes.Union(new ScopeViewModel[] {
                    GetOfflineAccessScope(Input.ScopesConsented.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess))
                });
            }
        }

        private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
        {
            return new ScopeViewModel
            {
                Name = identity.Name,
                DisplayName = identity.DisplayName,
                Description = identity.Description,
                Emphasize = identity.Emphasize,
                Required = identity.Required,
                Checked = check || identity.Required
            };
        }

        public ScopeViewModel CreateScopeViewModel(Scope scope, bool check)
        {
            return new ScopeViewModel
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Emphasize = scope.Emphasize,
                Required = scope.Required,
                Checked = check || scope.Required
            };
        }

        private ScopeViewModel GetOfflineAccessScope(bool check)
        {
            return new ScopeViewModel
            {
                Name = IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
                DisplayName = ConsentOptions.OfflineAccessDisplayName,
                Description = ConsentOptions.OfflineAccessDescription,
                Emphasize = true,
                Checked = check
            };
        }
    }
}