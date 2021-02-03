using System.Threading.Tasks;
using Plugins.CountlySDK.Enums;
using Plugins.CountlySDK.Models;

namespace Plugins.CountlySDK.Services
{
    public class InitializationCountlyService : AbstractBaseService
    {
        private readonly LocationService _locationService;
        private readonly SessionCountlyService _sessionService;

        internal InitializationCountlyService(CountlyConfiguration configuration, CountlyLogHelper logHelper, LocationService locationService, SessionCountlyService sessionCountlyService, ConsentCountlyService consentService) : base(configuration, logHelper, consentService)
        {
            _locationService = locationService;
            _sessionService = sessionCountlyService;
        }

        internal async Task OnInitialisationComplete()
        {
            await StartSession();
        }

        private async Task StartSession()
        {
            if (!_consentService.CheckConsent(Consents.Sessions)) {
                /* If location is disabled in init
                and no session consent is given. Send empty location as separate request.*/
                if (_locationService.IsLocationDisabled || !_consentService.CheckConsent(Consents.Location)) {
                    await _locationService.SendRequestWithEmptyLocation();
                } else {
                    /*
                 * If there is no session consent, 
                 * location values set in init should be sent as a separate location request.
                 */
                    await _locationService.SendIndependantLocationRequest();
                }
            } else if (!_configuration.EnableManualSessionHandling) {
                //Start Session
                await _sessionService.BeginSessionAsync();
            }
        }
    }
}