using API;
using FluentValidation;
using Sample;
using System;
using System.Linq;
using System.Net;
using System.Reflection;

namespace PxStat.Template
{
    /// <summary>
    /// Base Abstract class to allow for the template method pattern of Create, Read, Update and Delete objects from our model
    /// 
    /// </summary>
    public abstract class BaseTemplate_Read<T, V> : BaseTemplate<T, V>
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        protected BaseTemplate_Read(IRequest request, IValidator<T> validator) : base(request, validator)
        {
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="request"></param>
        /// <param name="validator"></param>
        public BaseTemplate_Read<T, V> Read()
        {
            try
            {

                //Run the parameters through the cleanse process
                dynamic cleansedParams;

                //If the API has the IndividualCleanseNoHtml attribute then parameters are cleansed individually
                //Any of these parameters whose corresponding DTO property contains the NoHtmlStrip attribute will not be cleansed of HTML tags

                bool isKeyValueParameters = Resources.Cleanser.TryParseJson<dynamic>(Request.parameters.ToString(), out dynamic canParse);

                if (API.MethodReader.MethodHasAttribute(Request.method, "NoCleanseDto"))
                {
                    cleansedParams = Request.parameters;
                }
                else
                {
                    if (!isKeyValueParameters)
                    {
                        cleansedParams = Resources.Cleanser.Cleanse(Request.parameters);
                    }
                    else
                    {
                        if (API.MethodReader.MethodHasAttribute(Request.method, "IndividualCleanseNoHtml"))
                        {
                            dynamic dto = GetDTO(Request.parameters);
                            cleansedParams = Resources.Cleanser.Cleanse(Request.parameters, dto);
                        }
                        else
                        {
                            cleansedParams = Resources.Cleanser.Cleanse(Request.parameters);
                        }
                    }
                }


                try
                {
                    DTO = GetDTO(cleansedParams);
                }
                catch
                {
                    throw new InputFormatException();
                }

                DTO = Resources.Sanitizer.Sanitize(DTO);

                DTOValidationResult = Validator.Validate(DTO);

                if (!DTOValidationResult.IsValid)
                {
                    OnDTOValidationError();

                    return this;
                }


                // The Actual Read should happen here by the specific class!
                if (!Execute())
                {
                    OnExecutionError();
                }


                if (!PostExecute())
                {
                    OnExecutionError();
                }

                FormatForRestful(Response);
                OnExecutionSuccess();

                return this;
            }

            catch (FormatException formatException)
            {
                //A FormatException error has been caught, log the error and return a message to the caller
                Log.Instance.Error(formatException);
                Response.error = formatException.Message ?? "Schema error";
                Response.statusCode = HttpStatusCode.InternalServerError;
                return this;
            }
            catch (Exception ex)
            {
                //An error has been caught,  log the error and return a message to the caller
                Log.Instance.Error(ex);
                Response.error = "Error";
                Response.statusCode = HttpStatusCode.InternalServerError;
                return this;
            }
            finally
            {
                Dispose();
            }
        }

        protected void FormatForRestful(IResponseOutput output)
        {

            if (Request.GetType().Equals(typeof(RESTful_API)))
            {

                if (Response.error != null)
                {
                    Response.statusCode = HttpStatusCode.InternalServerError;
                    Response.data = RESTful.FormatRestfulError(Response, null, HttpStatusCode.NoContent, Response.statusCode);
                }
                else
                    if (!Enum.IsDefined(typeof(HttpStatusCode), Response.statusCode))
                    Response.statusCode = HttpStatusCode.OK;
                //This is the default mimetype, may be amended before output by the calling BSO
                if (Response.mimeType == null)
                    Response.mimeType = "application/json";

                if (Response.data != null)
                {
                    Response.response = Response.data.ToString();
                    //We need to change e.g. an Excel file to a byte array
                    string data = Response.data.ToString();
                    if (data.Contains(";base64,"))
                    {
                        var base64Splits = data.Split(new[] { ";base64," }, StringSplitOptions.None);
                        var dataSplits = base64Splits[0].Split(new[] { "data:" }, StringSplitOptions.None);

                        // Override MimeType & Data
                        Response.mimeType = dataSplits[1];
                        Response.data = base64Splits[1];
                        Response.response = Utility.DecodeBase64ToByteArray(Response.data);
                    }
                }
                else
                {
                    Response.response = "";
                }
            }
        }

        /// <summary>
        /// Execution Success
        /// </summary>
        protected override void OnExecutionSuccess()
        {
            if (!Enum.IsDefined(typeof(HttpStatusCode), Response.statusCode))
                Response.statusCode = HttpStatusCode.OK;
            Log.Instance.Debug("Record created");
            //See if there's a cache in the process. If so then we need to flush the cache.

        }

        /// <summary>
        /// Execution Error
        /// </summary>
        protected override void OnExecutionError()
        {
             if (!Enum.IsDefined(typeof(HttpStatusCode), Response.statusCode))
                Response.statusCode =  HttpStatusCode.NotModified  ;
            Log.Instance.Debug("No record created");
        }

        /// <summary>
        /// Constructio
        /// </summary>
        /// <returns></returns>
        public BaseTemplate_Read<T, V> Create()
        {
            try
            {
                //Run the parameters through the cleanse process
                dynamic cleansedParams;


                //If the API has the IndividualCleanseNoHtml attribute then parameters are cleansed individually
                //Any of these parameters whose corresponding DTO property contains the NoHtmlStrip attribute will not be cleansed of HTML tags

                bool isKeyValueParameters = Resources.Cleanser.TryParseJson<dynamic>(Request.parameters.ToString(), out dynamic canParse);
                if(API.MethodReader.MethodHasAttribute(Request.method, "NoCleanseDto"))
                {
                    cleansedParams = Request.parameters;
                }
                else
                {
                    if (!isKeyValueParameters)
                    {
                        cleansedParams = Resources.Cleanser.Cleanse(Request.parameters);
                    }
                    else
                    {
                        if(API.MethodReader.MethodHasAttribute(Request.method, "IndividualCleanseNoHtml"))
                        {
                            dynamic dto = GetDTO(Request.parameters);
                            cleansedParams = Resources.Cleanser.Cleanse(Request.parameters, dto);
                        }
                        else
                        {
                            cleansedParams = Resources.Cleanser.Cleanse(Request.parameters);
                        }
                    }
                }



                try
                {
                    DTO = GetDTO(cleansedParams);
                }
                catch
                {
                    throw new InputFormatException();
                }

                DTO = Resources.Sanitizer.Sanitize(DTO);

                DTOValidationResult = Validator.Validate(DTO);


                if (!DTOValidationResult.IsValid)
                {
                    OnDTOValidationError(API.MethodReader.MethodHasAttribute(Request.method,"TokenSecure"));                
                    return this;
                }



                // Create the trace now that we're sure we have a SamAccountName if it exists
                // TODO This can be removed when the new tracing is tested
                // Trace_BSO_Create.Execute(Ado, Request, SamAccountName);

                // The Actual Creation should happen here by the specific class!
                if (!Execute())
                {
                    Ado.RollbackTransaction();
                    OnExecutionError();
                }
                Ado.CommitTransaction();

                if (!PostExecute())
                {
                    OnExecutionError();
                }
                OnExecutionSuccess();
                return this;
            }
            catch (FormatException formatException)
            {
                //A FormatException error has been caught, rollback the transaction, log the error and return a message to the caller
                Ado.RollbackTransaction();
                Log.Instance.Error(formatException);
                Response.statusCode = HttpStatusCode.BadRequest;
                Response.error = formatException.Message ?? "Schema error";
                return this;
            }
            catch (InputFormatException inputError)
            {
                //An error has been caught, rollback the transaction, log the error and return a message to the caller
                Ado.RollbackTransaction();
                Log.Instance.Error(inputError);
                Response.statusCode = HttpStatusCode.BadRequest;
                Response.error = "Schema error";
                return this;
            }
            catch (Exception ex)
            {
                //An error has been caught, rollback the transaction, log the error and return a message to the caller
                Ado.RollbackTransaction();
                Log.Instance.Error(ex);
                Response.statusCode = HttpStatusCode.InternalServerError;
                Response.error = "System error";
                return this;
            }
            finally
            {

                Dispose();
            }
        }
    }
}
