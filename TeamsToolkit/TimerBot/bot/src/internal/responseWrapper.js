// A wrapper to convert Azure Functions Response to Bot Builder's WebResponse.
class ResponseWrapper {
  socket;
  originalResponse;
  body;

  constructor(functionResponse) {
    this.socket = undefined;
    this.originalResponse = functionResponse;
  }

  end(...args) {
    // do nothing since res.end() is deprecated in Azure Functions.
  }

  send(body) {
    // record the body to be returned later.
    this.body = body;
  }

  status(status) {
    // call Azure Functions' res.status().
    return this.originalResponse?.status(status);
  }
}

module.exports = {
  ResponseWrapper,
};
