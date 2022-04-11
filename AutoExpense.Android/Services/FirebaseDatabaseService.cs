using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.Content;
using Firebase.Database;
using Firebase.Database.Query;
using FirebaseOptions = Firebase.Database.FirebaseOptions;
using static AutoExpense.Android.Helpers.Constants;

namespace AutoExpense.Android.Services
{
    public class FirebaseDatabaseService
    {
        private readonly FirebaseClient _firebaseClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="FirebaseDatabaseService"/> class
        /// </summary>
        /// <param name="context"></param>
        public FirebaseDatabaseService(Context context)
        {
            var options = new FirebaseOptions
            {
                // AuthTokenAsyncFactory = async () => await Task.FromResult(await GetToken(context))
            };

            _firebaseClient = new FirebaseClient(FIREBASE_DATABASE_URL, options);
        }

        /// <summary>
        ///     Get auth token for signed in user to use when making requests to the firebase database or storage
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task<string> GetToken(Context context)
        {
            //var tokenRequest = await new FirebaseAuthService(context).Auth.CurrentUser.GetIdToken(false);
            // var res = tokenRequest.JavaCast<GetTokenResult>();
            //return res.Token;

            return string.Empty;
        }

        /// <summary>
        ///     Add an item to the database. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item">Item to be added to the database</param>
        /// <param name="child">Root child in the database where the item will be added to</param>
        /// <param name="identifier">A child to the root child where the item will be added to</param>
        /// <returns></returns>
        public async Task<FirebaseObject<T>> AddItemAsync<T>(T item, string child, string identifier = null)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                var firebaseObject = await _firebaseClient
                    .Child(child)
                    .PostAsync(item, false);

                return firebaseObject;
            }
            else
            {
                var firebaseObject = await _firebaseClient
                    .Child(child)
                    .Child(identifier)
                    .PostAsync(item, false);

                return firebaseObject;
            }

        }

        /// <summary>
        ///     Get a list of items from the database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="child">Root child in the database where the item will be fetched from</param>
        /// <param name="identifier">A child to the root child where the item will be fe4tched from</param>
        /// <returns></returns>
        public async Task<List<T>> GetItemsAsync<T>(string child, string identifier = null)
        {
            var items = await GetRawItemsAsync<T>(child, identifier);
            return items.Select(i => i.Object).ToList();
        }

        public async Task<IReadOnlyCollection<FirebaseObject<T>>> GetRawItemsAsync<T>(string child, string identifier = null)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return await _firebaseClient
                    .Child(child)
                    .OnceAsync<T>();
            }

            return await _firebaseClient
                .Child(child)
                .Child(identifier)
                .OnceAsync<T>();
        }

        public async Task UpdateItemAsync<T>(T item, string child, string key, string identifier = null)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                await _firebaseClient
                    .Child($"{child}/{key}")
                    .PutAsync(item);
            }
            else
            {
                await _firebaseClient
                    .Child(child)
                    .Child($"{identifier}/{key}")
                    .PutAsync(item);
            }
        }

        public async Task DeleteItemAsync(string child, string identifier = null, string key = null)
        {
            if (!string.IsNullOrEmpty(identifier) && string.IsNullOrEmpty(key))
            {
                await _firebaseClient
                    .Child(child)
                    .Child(identifier)
                    .DeleteAsync();
            }

            else if (!string.IsNullOrEmpty(key) && string.IsNullOrEmpty(identifier))
            {
                await _firebaseClient
                    .Child(child)
                    .Child(key)
                    .DeleteAsync();
            }

            else if (!string.IsNullOrEmpty(identifier) && !string.IsNullOrEmpty(key))
            {
                await _firebaseClient
                    .Child(child)
                    .Child(identifier)
                    .Child(key)
                    .DeleteAsync();
            }
            else
            {
                await _firebaseClient.Child(child)
                    .DeleteAsync();
            }

        }

    }
}