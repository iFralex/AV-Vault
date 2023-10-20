using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.DynamicLinks;
using System;
using SocialApp;
using Firebase.DynamicLinks;

public class DynamicsLinkController : MonoBehaviour
{
    void Awake()
    {
        /*Application.deepLinkActivated += OnDynamicLink;
        if (Application.absoluteURL != string.Empty)
            StartCoroutine(Aspetta());
        */
        DynamicLinks.DynamicLinkReceived += OnDynamicLink;
    }/*
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            OnDynamicLink("memeId=-MtQHvOT41oZD0kuQgES");
    }*/
    IEnumerator Aspetta()
    {
        yield return new WaitWhile(() => !AppManager.FIREBASE_CONTROLLER.IsFirebaseInited());
        yield return new WaitForSeconds(.1f);
        //OnDynamicLink(Application.absoluteURL);
    }

    void OnDynamicLink(object sender, EventArgs args)
    {
        print("ciao");
        var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;
        Debug.Log("Received dynamic link " + dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString);
        string link = dynamicLinkEventArgs.ReceivedDynamicLink.Url.OriginalString.Split(char.Parse("="))[1];
        AppSettings.memeCondivisoKey = link;
        AppManager.VIEW_CONTROLLER.ShowSinglePost();
    }

    public void OnDynamicLink(string url)
    {
        if (url.Contains("memeId"))
        {
            string link = url.Split(char.Parse("="))[1];
            AppSettings.memeCondivisoKey = link;
            AppManager.VIEW_CONTROLLER.ShowSinglePost();
        }
    }

    /*public void OnDynamicLink(string url)
    {
        string link = url.Split(char.Parse("="))[1];
        AppSettings.memeCondivisoKey = link;
        AppManager.VIEW_CONTROLLER.ShowSinglePost();
        print("arrivato: " + link);
    }*/

    public void CondividiLink(FeedViewController ogg)
    {
        var components = new DynamicLinkComponents(new Uri("https://www.altavelos.net/?memeId=" + ogg.LoadedFeed.Key), "https://avvaultsocial.page.link")
        {
            IOSParameters = new IOSParameters("com.ShilohCuevas.AVVaultSocial") { AppStoreId = "1553868265", BundleId = "com.ShilohCuevas.AVVaultSocial"},
            AndroidParameters = new AndroidParameters("com.ShilohCuevas.AVVault"),
            SocialMetaTagParameters = new SocialMetaTagParameters() { Title = "This meme of " + ogg.ProfileUseeNameLabel.text + " on AV: Vault is wanderfull!" }//, Description = "See this meme and other memes on the AV: Vault app." }
        };
        print(components.LongDynamicLink.OriginalString + "&efr=1");
        new NativeShare().SetUrl(components.LongDynamicLink.OriginalString + "&efr=1").Share();
    }

    void CreaLink()
    {
        var components = new DynamicLinkComponents(new Uri("https://www.altavelos.net?meme="), "https://avvault.page.link")
        {
            IOSParameters = new IOSParameters("com.ShilohCuevas.AVVaultSocial")
            { AppStoreId = "1553868265", BundleId = "com.ShilohCuevas.AVVaultSocial", CustomScheme = "avvault://"},
            AndroidParameters = new AndroidParameters("com.ShilohCuevas.AVVault") { PackageName = "com.ShilohCuevas.AVVault"},
            Link = new Uri("https://apple.com")
            
            
        };
        string link = components.LongDynamicLink.ToString();
        link += "&ofl=https://altavelos.net&efr=1";
        print(link);
    }
}