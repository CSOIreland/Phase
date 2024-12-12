using API;

using Microsoft.AspNetCore.HttpOverrides;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using CSO.AutoMapper;
using FluentValidation.Results;

namespace PxStat.Template
{
    /// <summary>
    /// Base Abstract class to allow for the template method pattern of Create, Read, Update and Delete objects from our model
    /// 
    /// </summary>
    public abstract class BaseTemplate<T, V>
    {
        #region Properties
        /// <summary>
        /// IADO variable
        /// </summary>
        public IADO Ado { get; }


        /// <summary>
        /// Account username
        /// </summary>
        public string SamAccountName { get; set; }

        /// <summary>
        /// Request passed into the API
        /// </summary>
        public IRequest Request { get; }

        /// <summary>
        /// Response passed back by API
        /// </summary>
        public IResponseOutput Response { get; set; }

        /// <summary>
        /// DTO created from request parameters
        /// </summary>
        protected T DTO { get; set; }

        /// <summary>
        /// Validator (Fluent Validation)
        /// </summary>
        protected IValidator<T> Validator { get; }

        /// <summary>
        /// Validation result
        /// </summary>
        protected ValidationResult DTOValidationResult { get; set; }


        protected string UserIdentifier { get; set; }

       

        #endregion 

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        public  BaseTemplate(IRequest request, IValidator<T> validator)
        {

   

            if (ActiveDirectory.IsAuthenticated(request.userPrincipal))
            {
                SamAccountName = request.userPrincipal.SamAccountName.ToString();
            }

            Request = request;
            if (Request.method.Equals("HEAD"))
            {
                Response = new RESTful_Output();
            }
            else
                Response = new JSONRPC_Output();
            Validator = validator;

           
            

        }

        /// <summary>
        /// Dispose of the IADO for connection tidy-up
        /// </summary>
        protected void Dispose()
        {


        }

   

        public virtual bool PostExecute()
        {
            return true;
        }

        #region Abstract methods.
        // These methods must be overriden.
        abstract protected bool Execute();

        /// <summary>
        /// Return the current DTO
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        virtual protected T GetDTO(dynamic parameters = null)
        {
            AutoMap.Mapper = AutoMap.CreateMapper(ApiServicesHelper.ApiConfiguration.Settings);
            dynamic copyParams;
            try
            {
                // Mapster would be better but autmapper used in HSM
                copyParams = parameters ?? Request.parameters;
                //ExpandoObject? mappedParamers = ((JObject)copyParams).ToObject<ExpandoObject>();
                return AutoMap.Mapper.Map<T>(copyParams);
            }
            catch (Exception ex)
            {
                Log.Instance.ErrorFormat("GetDTO error for {0}", typeof(T));
                if (ex.InnerException != null)
                {
                    if (ex.InnerException is FormatException)
                    {
                        throw ex.InnerException;
                    }
                    else
                    {
                        Log.Instance.Error(ex.InnerException);
                    }
                }
                else
                {
                    // throw ex;
                    return parameters != null ?
                    (T)Activator.CreateInstance(typeof(T), parameters) :
                    (T)Activator.CreateInstance(typeof(T), Request.parameters);
                }

                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        abstract protected void OnExecutionSuccess();

        /// <summary>
        /// 
        /// </summary>
        abstract protected void OnExecutionError();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        abstract protected bool HasPrivilege();

        #endregion

        #region Default methods.
        // These methods can be left as their default implementations.

        /// <summary>
        /// Validation fail
        /// </summary>
        virtual protected void OnDTOValidationError(bool isMachineReadable=false)
        {
            //parameter validation not ok - return an error and proceed no further
            Log.Instance.Debug("Validation failed: " + JsonConvert.SerializeObject(DTOValidationResult.Errors));

            if (!isMachineReadable)
            {
                Response.error = "Validation error";
                Response.statusCode=HttpStatusCode.BadRequest;  
            }
            else
            {
                Response.error = DTOValidationResult.Errors;
                Response.statusCode=HttpStatusCode.BadRequest;  
            }
        }



 

        /// <summary>
        /// 
        /// </summary>
        virtual protected void OnPrivilegeSuccess()
        {
            Log.Instance.Debug("Valid privilege");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        virtual protected bool HasUserToBeAuthenticated()
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
    


    }

    public class InputFormatException : Exception
    {
        public InputFormatException() : base("Invalid format found in input parameters")
        {

        }
    }
}
#endregion