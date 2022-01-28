using SoftGear.Strix.Unity.Runtime;
using UnityEngine;

using System.Collections.Generic;

public class StrixDeleteRoomExample : MonoBehaviour
{
    void Start()
    {
        var strixNetwork = StrixNetwork.instance;

        // これは仮の値です。実際のアプリケーションIDに変更してください
        // Strix Cloudのアプリケーション情報タブにあります: https://www.strixcloud.net/app/applist
        strixNetwork.applicationId = "874c60ba-0715-4997-b0ac-23779f07cc26";

        strixNetwork.ConnectMasterServer(
            // これは仮の値です。実際のマスターホスト名に変更してください。
            // Strix Cloudのアプリケーション情報タブにあります: https://www.strixcloud.net/app/applist
            host: "c0474a6d1084d90673c8c991.game.strixcloud.net",
            connectEventHandler: _ => {
                Debug.Log("Connection established.");

                strixNetwork.CreateRoom(
                    new Dictionary<string, object> {
                        { "name", "My Game Room" },
                        { "capacity", 20 }
                    },
                    playerName: "My Player Name",
                    handler: createRoomResult => {
                        Debug.Log("Room created.");

                        strixNetwork.DeleteRoom(
                            roomId: strixNetwork.room.GetPrimaryKey(),
                            handler: deleteRoomResult => Debug.Log("Room deleted: " + (strixNetwork.room == null)),
                            failureHandler: deleteRoomError => Debug.LogError("Could not delete room.Reason: " + deleteRoomError.cause)
                        );
                    },
                    failureHandler: createRoomError => Debug.LogError("Could not create room.Reason: " + createRoomError.cause)
                );
            },
            errorEventHandler: connectError => Debug.LogError("Connection failed.Reason: " + connectError.cause)
        );
    }
}