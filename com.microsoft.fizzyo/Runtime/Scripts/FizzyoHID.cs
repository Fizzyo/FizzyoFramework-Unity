// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if WINDOWS_UWP
using System;
using System.Collections.Generic;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
#endif

public class FizzyoHID
{
    //Singleton
    private static FizzyoHID _instance;

    public static FizzyoHID Instance()
    {
        if (_instance == null)
        {
            _instance = new FizzyoHID();
        }
        return _instance;
    }

    private double _currentPressureValue = 0;

    public double CurrentPressureValue
    {
        get
        {
            return _currentPressureValue;
        }
    }

#if WINDOWS_UWP

    private DeviceWatcher watcher = null;

    readonly Dictionary<string, HidDevice> _gamepads = new Dictionary<string, HidDevice>();

    public IReadOnlyDictionary<string, HidDevice> Gamepads => _gamepads;

    public FizzyoHID()
    {
        var deviceSelector = HidDevice.GetDeviceSelector(0x01, 0x04);

        watcher = DeviceInformation.CreateWatcher(deviceSelector);
        watcher.Added += HandleAdded;
        watcher.Removed += HandleRemoved;
        watcher.Start();
    }

    private async void HandleAdded(DeviceWatcher sender, DeviceInformation args)
    {
        try
        {
            HidDevice hidDevice = await HidDevice.FromIdAsync(args.Id, FileAccessMode.Read);
            hidDevice.InputReportReceived += HandleInputReportRecieved;
            _gamepads.Add(args.Id, hidDevice);
        }
        catch
        {
            return;
        }
    }

    private void HandleRemoved(DeviceWatcher sender, DeviceInformationUpdate args)
    {
        foreach (KeyValuePair<string, HidDevice> pair in Gamepads)
        {
            if (pair.Key.Equals(args.Id))
            {
                _gamepads.Remove(pair.Key);
                return;
            }
        }
    }

    void HandleInputReportRecieved(HidDevice sender, HidInputReportReceivedEventArgs args)
    {
        try
        {
            _currentPressureValue = args.Report.GetNumericControl(0x01, 0x30).Value;
        }
        catch
        {
            return;
        }
    }

#endif 
}