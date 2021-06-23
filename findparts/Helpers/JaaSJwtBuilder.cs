using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography.X509Certificates;
using JWT.Builder;
using Findparts.Core;
using JWT.Algorithms;

namespace Findparts.Helpers
{
    /// <summary>
    /// JaaSJwtBuilder class that helps generate JaaS tokens.
    /// </summary>
    public class JaaSJwtBuilder
    {

        /// <summary>
        /// To be used with exp value.
        /// The time after which the JWT expires.
        /// </summary>
        public const double EXP_TIME_DELAY_SEC = 7200;

        /// <summary>
        /// To be used with nbf value.
        /// The time before which the JWT must not be accepted for processing.
        /// </summary>
        public const double NBF_TIME_DELAY_SEC = 10;

        private readonly IDictionary<String, Object> userClaims = new Dictionary<String, Object>();
        private readonly IDictionary<String, Object> featureClaims = new Dictionary<String, Object>();
        private readonly IDictionary<String, Object> payload = new Dictionary<String, Object>();
        private String apiKey = String.Empty;

      
        private JaaSJwtBuilder() { }

        /// <summary
        /// Creates a new JaaSJwtBuilder.
        /// <returns>
        /// A new builder that needs to be setup.
        /// </returns>
        /// </summary>
        public static JaaSJwtBuilder Builder()
        {
            var jJB = new JaaSJwtBuilder();
            jJB.payload.Add("aud", "jitsi");
            jJB.payload.Add("iss", "chat");
            return jJB;
        }

        /// <summary>
        /// Sets the value for the kid header claim. Represents the JaaS api key.
        /// You can find the api key here : https://jaas.8x8.vc/#/apikeys
        /// </summary>
        /// <param name="apiKey">Your API Key</param>
        /// <returns>
        /// A builder with kid claim set.
        /// </returns>
        public JaaSJwtBuilder WithApiKey(String apiKey)
        {
            this.apiKey = apiKey;
            return this;
        }

        /// <summary>
        /// Sets the value for the user avatar url as a string.
        /// </summary>
        /// <param name="url">Url for user avatar</param>
        /// <returns>
        /// A builder with avatar claim set.
        /// </returns>
        public JaaSJwtBuilder WithUserAvatar(String url)
        {
            userClaims.Add("avatar", url);
            return this;
        }

        /// <summary>
        /// Sets the value for the moderator claim.
        /// </summary>
        /// <param name="isModerator">
        /// If value is true user is moderator, false otherwise.
        /// </param>
        /// <returns>
        /// A builder with moderator claim set.
        /// </returns>
        public JaaSJwtBuilder WithModerator(bool isModerator)
        {
            userClaims.Add("moderator", isModerator ? "true" : "false");
            return this;
        }

        /// <summary>
        /// Sets the value for the user name to be displayed in the meeting.
        /// </summary>
        /// <param name="userName">
        /// User name to be displayed in meeting.
        /// </param>
        /// <returns>
        /// A builder with name claim set.
        /// </returns>
        public JaaSJwtBuilder WithUserName(String userName)
        {
            userClaims.Add("name", userName);
            return this;
        }

        /// <summary>
        /// Sets the value for the user email claim.
        /// </summary>
        /// <param name="userEmail">
        /// User email to be used in meeting.
        /// </param>
        /// <returns>
        /// A builder with email claim set.
        /// </returns>
        public JaaSJwtBuilder WithUserEmail(String userEmail)
        {
            userClaims.Add("email", userEmail);
            return this;
        }

        /// <summary>
        /// Sets the value for the live streaming feature claim.
        /// </summary>
        /// <param name="isEnabled">
        /// If value is true recording is enabled, false otherwise.
        /// </param>
        /// <returns>
        /// A builder with livestreaming claim set.
        /// </returns>
        public JaaSJwtBuilder WithLiveStreamingEnabled(bool isEnabled)
        {
            featureClaims.Add("livestreaming", (isEnabled ? "true" : "false"));
            return this;
        }

        /// <summary>
        /// Sets the value for the recording feature claim.
        /// </summary>
        /// <param name="isEnabled">
        /// If value is true recording is enabled, false otherwise.
        /// </param>
        /// <returns>
        /// A builder with recording claim set.
        /// </returns>
        public JaaSJwtBuilder WithRecordingEnabled(bool isEnabled)
        {
            featureClaims.Add("recording", isEnabled ? "true" : "false");
            return this;
        }

        /// <summary>
        /// Sets the value for the outbound-call feature claim.
        /// </summary>
        /// <param name="isEnabled">
        /// If value is true outbound calls are enabled, false otherwise.
        /// </param>
        /// <returns>
        /// A builder with outbound-call claim set.
        /// </returns>
        public JaaSJwtBuilder WithOutboundCallEnabled(bool isEnabled)
        {
            featureClaims.Add("outbound-call", (isEnabled ? "true" : "false"));
            return this;
        }

        /// <summary>
        /// Sets the value for the transcription feature claim.
        /// </summary>
        /// <param name="isEnabled">
        /// If value is true transcription is enabled, false otherwise.
        /// </param>
        /// <returns>
        /// A builder with transcription claim set.
        /// </returns>
        public JaaSJwtBuilder WithTranscriptionEnabled(bool isEnabled)
        {
            featureClaims.Add("transcription", (isEnabled ? "true" : "false"));
            return this;
        }

        /// <summary>
        /// Sets the value for the exp claim representing the time until the token expires.
        /// You don't have to change this value too much, use defaults.
        /// </summary>
        /// <param name="expTime"></param>
        /// <returns>
        /// A builder with exp claim set.
        /// </returns>
        public JaaSJwtBuilder WithExpTime(DateTime expTime)
        {
            payload.Add("exp", new DateTimeOffset(expTime).ToUnixTimeSeconds());
            return this;
        }

        /// <summary>
        /// Sets the value for the nbf claim.
        /// You don't have to change this value too much, use defaults.
        /// </summary>
        /// <param name="nbfTime"></param>
        /// <returns>
        /// A builder with nbf claim set.
        /// </returns>
        public JaaSJwtBuilder WithNbfTime(DateTime nbfTime)
        {
            payload.Add("nbf", new DateTimeOffset(nbfTime).ToUnixTimeSeconds());
            return this;
        }

        /// <summary>
        /// Sets the value for the room name.
        /// This field supports also wildcard("*") if the token is issued for all rooms.
        /// </summary>
        /// <param name="roomName">
        /// The meeting room value for which the token is issued.
        /// </param>
        /// <returns>
        /// A builder with room claim set.
        /// </returns>
        public JaaSJwtBuilder WithRoomName(String roomName)
        {
            payload.Add("room", roomName);
            return this;
        }

        /// <summary>
        /// Sets the value for the sub claim representing the AppID ( previously tenant name/unique identifier).
        /// </summary>
        /// <param name="appId">The AppID that identifies your application.</param>
        /// <returns></returns>
        public JaaSJwtBuilder WithAppID(String appId)
        {
            payload.Add("sub", appId);
            return this;
        }

        /// <summary>
        /// Sets the value for the id claim.
        /// </summary>
        /// <param name="id">
        /// The user's unique identifier.
        /// </param>
        /// <returns>
        /// A builder with kid claim set.
        /// </returns>
        public JaaSJwtBuilder WithUserId(String id)
        {
            userClaims.Add("id", id);
            return this;
        }

        /// <summary>
        /// Fills the default values for required claims.
        /// </summary>
        /// <returns>
        /// A builder with needed claim set to default values.
        /// </returns>
        public JaaSJwtBuilder WithDefaults()
        {
            return WithExpTime(DateTime.UtcNow.AddSeconds(EXP_TIME_DELAY_SEC))
                .WithNbfTime(DateTime.UtcNow.AddSeconds(-NBF_TIME_DELAY_SEC))
                .WithLiveStreamingEnabled(true)
                .WithRecordingEnabled(true)
                .WithOutboundCallEnabled(true)
                .WithTranscriptionEnabled(true)
                .WithModerator(true)
                .WithRoomName("*")
                .WithUserId(System.Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Generate a signed JaaS JWT token string.
        /// </summary>
        /// <param name="privateKey">The private key used to sign the JWT.</param>
        /// <returns></returns>
        public String Encode()
        {
            var certFile = new X509Certificate2(Config.JitsiPrivateKeyPath, "", X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
            
            var context = new Dictionary<String, Object>();
            context.Add("user", userClaims);
            context.Add("features", featureClaims);
            payload.Add("context", context);

            

            var token = new JwtBuilder()
               .WithAlgorithm(new RS256Algorithm(certFile.GetRSAPublicKey(), certFile.GetRSAPrivateKey()))
               .AddHeader(HeaderName.KeyId, apiKey)
               .AddClaims(payload.ToList())
               .Encode();

            return token;
        }
    }
}