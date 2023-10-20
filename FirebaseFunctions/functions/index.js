/**
 * Copyright 2017 Google Inc. All Rights Reserved.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
'use strict';

const functions = require('firebase-functions');
const gcs = require('@google-cloud/storage');
const path = require('path');
const os = require('os');
const fs = require('fs');
const ffmpeg = require('fluent-ffmpeg');
const ffmpeg_static = require('ffmpeg-static');
const admin = require('firebase-admin');
admin.initializeApp(functions.config().firebase);

// Makes an ffmpeg command return a promise.
function promisifyCommand(command) {
    return new Promise((resolve, reject) => {
        command.on('end', resolve).on('error', reject).run();
    });
}

const runtimeOpts = {
    timeoutSeconds: 540,
    memory: '2GB'
}

const vdeioUploadOpts = {
    timeoutSeconds: 540,
    memory: '2GB',
	resumable: false
}

exports.UploadAndCompressVideo = functions.runWith(vdeioUploadOpts).https.onCall(async (data) => {
    console.log('Start call UploadAndCompressVideo');
    console.log("path = " + data.uploadPath);
    console.log("bucket = " + data.bucketUrl);

    const fileBucket = data.bucketUrl; // The Storage bucket that contains the file.
    const filePath = data.uploadPath; // File path in the bucket.
    const databasePath = data.databasePath;
    // Get the file name.
    const fileName = path.basename(filePath);
    const urlDBRef = admin.database().ref("/" + databasePath);
    console.log("database url = " + data.databasePath);


    // Download file from bucket.
    const bucket = admin.storage().bucket(fileBucket);
    //const bucket = gcs.bucket(fileBucket);
    const tempFilePath = path.join(os.tmpdir(), fileName);
    // We add a '_output.flac' suffix to target audio file name. That's where we'll upload the converted audio.
    const targetTempFileName = fileName.replace(/\.[^/.]+$/, '') + '_output.mp4';
    const targetTempFilePath = path.join(os.tmpdir(), targetTempFileName);
    const targetStorageFilePath = path.join(path.dirname(filePath), targetTempFileName);

    await bucket.file(filePath).download({ destination: tempFilePath });
    console.log('Video downloaded locally to', tempFilePath);
    // Convert the audio to mono channel using FFMPEG.

    let command = ffmpeg(tempFilePath)
        .setFfmpegPath(ffmpeg_static.path)
        .videoBitrate(1024)
        .videoCodec('libx264')
        .format('mp4')
        .output(targetTempFilePath);

    await promisifyCommand(command);
    console.log('Output video created at', targetTempFilePath);

    const options = {
        destination: targetStorageFilePath,
        predefinedAcl: 'publicRead',
		resumable: false
    };
    await bucket.upload(targetTempFilePath, options).then(result => {
        const file = result[0];
        return file.getMetadata();
    }).then(results => {
        const metadata = results[0];
        console.log('metadata=', metadata.mediaLink);
        urlDBRef.set(metadata.mediaLink);
        console.log("database updated");
        return null;
    }).catch(error => {
        console.error(error);
    });
    console.log('Output video uploaded to', targetStorageFilePath);


    fs.unlinkSync(tempFilePath);
    fs.unlinkSync(targetTempFilePath);

    console.log('Temporary files removed.', targetTempFilePath);
});
/*
// friends count
exports.countfriendchange = functions.database.ref('/UserFriends/{userId}/List/{friendID}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');

        let increment;
        if (change.after.exists() && !change.before.exists()) {
            increment = 1;
        } else if (!change.after.exists() && change.before.exists()) {
            increment = -1;
        } else {
            return null;
        }

        await countRef.transaction((current) => {
            return (current || 0) + increment;
        });
        console.log('Counter updated.');
        return null;
    });

exports.recountfriendscount = functions.database.ref('/UserFriends/{userId}/Count').onDelete(async (snap) => {
    const counterRef = snap.ref;
    const collectionRef = counterRef.parent.child('List');

    const messagesData = await collectionRef.once('value');
    return await counterRef.set(messagesData.numChildren());
});


// friends requested count
exports.countrequestedchange = functions.database.ref('/UserRequestedFriends/{userId}/List/{friendID}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');

        let increment;
        if (change.after.exists() && !change.before.exists()) {
            increment = 1;
        } else if (!change.after.exists() && change.before.exists()) {
            increment = -1;
        } else {
            return null;
        }

        await countRef.transaction((current) => {
            return (current || 0) + increment;
        });
        console.log('Counter updated.');
        return null;
    });

exports.recountrequestedfriend = functions.database.ref('/UserRequestedFriends/{userId}/Count').onDelete(async (snap) => {
    const counterRef = snap.ref;
    const collectionRef = counterRef.parent.child('List');

    const messagesData = await collectionRef.once('value');
    return await counterRef.set(messagesData.numChildren());
});


// friends pending count
exports.countpendingfriendchange = functions.database.ref('/UserPendingFriends/{userId}/List/{friendID}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');

        let increment;
        if (change.after.exists() && !change.before.exists()) {
            increment = 1;
        } else if (!change.after.exists() && change.before.exists()) {
            increment = -1;
        } else {
            return null;
        }

        await countRef.transaction((current) => {
            return (current || 0) + increment;
        });
        console.log('Counter updated.');
        return null;
    });

exports.recountpendingfriend = functions.database.ref('/UserPendingFriends/{userId}/Count').onDelete(async (snap) => {
    const counterRef = snap.ref;
    const collectionRef = counterRef.parent.child('List');

    const messagesData = await collectionRef.once('value');
    return await counterRef.set(messagesData.numChildren());
});

*/
// post likes count
exports.countpostlikeschange = functions.database.ref('/PostLikes/{postId}/List/{userId}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');

        let increment;
        if (change.after.exists() && !change.before.exists()) {
            increment = 1;
        } else if (!change.after.exists() && change.before.exists()) {
            increment = -1;
        } else {
            return null;
        }

        await countRef.transaction((current) => {
            return (current || 0) + increment;
        });
        console.log('Counter updated.');
        return null;
    });

exports.recountpostlikes = functions.database.ref('/PostLikes/{postId}/Count').onDelete(async (snap) => {
    const counterRef = snap.ref;
    const collectionRef = counterRef.parent.child('List');

    const messagesData = await collectionRef.once('value');
    return await counterRef.set(messagesData.numChildren());
});

// post comments count
exports.countpostcommentschange = functions.database.ref('/PostComments/{postId}/List/{msgId}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');

        let increment;
        if (change.after.exists() && !change.before.exists()) {
            increment = 1;
        } else if (!change.after.exists() && change.before.exists()) {
            increment = -1;
        } else {
            return null;
        }

        await countRef.transaction((current) => {
            return (current || 0) + increment;
        });
        console.log('Counter updated.');
        return null;
    });

exports.recountpostcomments = functions.database.ref('/PostComments/{postId}/Count').onDelete(async (snap) => {
    const counterRef = snap.ref;
    const collectionRef = counterRef.parent.child('List');

    const messagesData = await collectionRef.once('value');
    return await counterRef.set(messagesData.numChildren());
});

/*
exports.SharePostWithFriends = functions.runWith(runtimeOpts).https.onCall(async (data) => {

    const _userId = data._userId;
    const _postId = data._postId;
	console.log("StartGetFriends");
    admin.database().ref('UserFriends/' + _userId + '/List').once('value')
        .then(snapshot => {
			console.log("GetFriends");

            snapshot.forEach(childSnapshot => {
				console.log(key);
                var key = childSnapshot.key;

                admin.database().ref().child('FriendsPosts/' + key + '/List/' + _postId).set(0);

            });
            return null;
        }).catch(error => {
            console.log(error);
        });

});

exports.GetServerTimeStamp = functions.runWith(runtimeOpts).https.onCall(async (data) => {
    return Date.now();

});

// friends post count
exports.countfriendspostchange = functions.database.ref('/FriendsPosts/{userId}/List/{postId}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');

        let increment;
        if (change.after.exists() && !change.before.exists()) {
            increment = 1;
        } else if (!change.after.exists() && change.before.exists()) {
            increment = -1;
        } else {
            return null;
        }

        await countRef.transaction((current) => {
            return (current || 0) + increment;
        });
        return null;
    });
*/
// On Create post
exports.onaddpost = functions.database.ref('/AllPosts/{postId}').onCreate(async (change, context) => {
	let userID = "";
	let postID = context.params.postId;

	const userIDRef = admin.database().ref('AllPosts/' + postID + '/OwnerID');
	await userIDRef.once('value')
	  .then(snapshot => {
		  userID = snapshot.val();
		  return null;
	  });
	admin.database().ref('UserFriends/' + userID + '/List').once('value')
        .then(snapshot => {
			console.log("GetFriends");

            snapshot.forEach(childSnapshot => {

                var key = childSnapshot.key;
                admin.database().ref().child('FriendsPosts/' + key + '/List/' + postID).set(0);

            });
            return null;
        }).catch(error => {
            console.log(error);
        });
});
/*
// all unread message count
exports.countunreadmessagechange = functions.database.ref('/UserMessages/{usersId}/List/{msgId}').onWrite(
  async (change, context) => {
  let userID = "";
  console.log(change.after.ref);
  const targetIDRef = change.after.ref.child('TargetId');
  const userIDRef = change.after.ref.child('UserID');

  await userIDRef.once('value')
  .then(snapshot => {
      userID = snapshot.val();
	  console.log("UserID " + userID);
	  return null;
  });


  await targetIDRef.once('value')
  .then(snapshot => {
      const targetId = snapshot.val();
	  console.log("TargetID " + targetId);
	  admin.database().ref().child('UserMessagesGroups').child(targetId).child('Users').once('value')
	  .then(snapshot2 => {

      const promises = [];

      snapshot2.forEach(childSnapshot => {

          var _userId = childSnapshot.val();

		  console.log("list user " + _userId);

		  if (_userId !== userID)
		  {
			  console.log("update user " + _userId);
			  const countRef = admin.database().ref().child('UnreadMessages').child(_userId).child("List").child(targetId);

			  let increment;
			  if (change.after.exists() && !change.before.exists()) {
				increment = 1;
			  } else if (!change.after.exists() && change.before.exists()) {
				increment = -1;
			  } else {
				return null;
			  }

			  countRef.transaction((current) => {
				return (current || 0) + increment;
			  });
			  return null;
		  }
		});
	  return null;
	  }).catch(error => {  });
	  return null;
  });
});


// all unread message count
exports.countallunreadfriendschange = functions.database.ref('/UnreadMessages/{userId}/List/{friendId}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');
        let count = 0;
        await collectionRef.once('value')
            .then(snapshot => {

                snapshot.forEach(function (childSnapshot) {

                    count += childSnapshot.val();

                })
                return null;
            });
        countRef.set(count);

    });
*/
// all post interactions count
exports.countpostinteractionschange = functions.database.ref('/PostInteractions/{userId}/List/{postId}').onWrite(
    async (change) => {
        const collectionRef = change.after.ref.parent;
        const countRef = collectionRef.parent.child('Count');
        let count = 0;
        await collectionRef.once('value')
            .then(snapshot => {

                snapshot.forEach(function (childSnapshot) {

                    count += childSnapshot.val();

                })
                return null;
            });
        countRef.set(count);
        console.log("ciao");
    });

//send notification
exports.SendFCM = functions.runWith(runtimeOpts).https.onCall(async (data) => {

    const _userId = data._userId;
    const _title = data._title;
    const _body = data._body;
    let tokensSnapshot;
    await admin.database().ref(`/DeviceTokens/` + _userId).once('value')
        .then(snapshot => {

            tokensSnapshot = snapshot;
            return null;
        });

    let tokens;
    // Notification details.
    const payload = {
        notification: {
            title: _title,
            body: _body
        }
    };

    // Listing all tokens as an array.
    tokens = Object.keys(tokensSnapshot.val());

    await admin.messaging().sendToDevice(tokens, payload);

});

exports.scheduledFunction = functions.pubsub.schedule('30 18 * * *').onRun((context) => {
    await admin.messaging().sendToDevice(tokens, payload);
  return null;
});
