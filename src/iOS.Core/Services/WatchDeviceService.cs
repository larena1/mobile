﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bit.App.Services;
using Bit.Core.Abstractions;
using Bit.Core.Models;
using Newtonsoft.Json;
using WatchConnectivity;

namespace Bit.iOS.Core.Services
{
    public class WatchDeviceService : BaseWatchDeviceService
    {
        public WatchDeviceService(ICipherService cipherService,
            IEnvironmentService environmentService,
            IStateService stateService,
            IVaultTimeoutService vaultTimeoutService)
            : base(cipherService, environmentService, stateService, vaultTimeoutService)
        {
        }

        public override bool IsConnected => WCSessionManager.SharedManager.IsSessionActivated;

        protected override bool CanSendData => WCSessionManager.SharedManager.IsValidSession;

        protected override bool IsSupported => WCSession.IsSupported;

        protected override Task SendDataToWatchAsync(WatchDTO watchDto)
        {
            var serializedData = JsonConvert.SerializeObject(watchDto);

            // Add time to the key to make it change on every message sent so it's delivered faster.
            // If we use the same key then the OS may defer the delivery of the message because of
            // resources, reachability and other stuff
            WCSessionManager.SharedManager.SendBackgroundHighPriorityMessage(new Dictionary<string, object>
            {
                [$"watchDto-{DateTime.UtcNow.ToLongTimeString()}"] = serializedData
            });

            return Task.CompletedTask;
        }

        protected override void ConnectToWatch()
        {
            WCSessionManager.SharedManager.StartSession();
        }
    }
}