// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using UnityEngine.Networking;

namespace Fizzyo
{
    public static class FizzyoNetworking
    {
        public static string ApiEndpoint = "api/v1/";

        public static LoginReturnType loginResult = LoginReturnType.FAILED_TO_CONNECT;

        public static UnityWebRequest PostWebRequest(string path, Dictionary<string,string> formData)
        {
            UnityWebRequest unityWebRequest = UnityWebRequest.Post(FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + path, formData);

            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + FizzyoFramework.Instance.User.AccessToken);
            unityWebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");

            return unityWebRequest;
        }

        public static UnityWebRequest GetWebRequest(string path)
        {
            UnityWebRequest unityWebRequest = UnityWebRequest.Get(FizzyoFramework.Instance.FizzyoConfigurationProfile.ApiPath + path);

            unityWebRequest.SetRequestHeader("Authorization", "Bearer " + FizzyoFramework.Instance.User.AccessToken);
            unityWebRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");

            return unityWebRequest;
        }
    }
}