/**
 * Header that is sent to the server or received by the client
 * during communication.
 *
 * @interface
 */
declare interface Header {
  /**
   * Name of the header.
   *
   * @example Accept
   * @example Authorization
   * @example X-Custom-Header
   */
  name: string;

  /**
   * Value of the header.
   */
  value: string;
}

/**
 * Http request type that contains the basic
 * information about the executed request.
 *
 * @interface
 */
declare interface HttpRequest {
  /**
   * String representation of the used http method.
   */
  method: 'GET' | 'PUT' | 'POST' | 'DELETE' | 'HEAD';

  /**
   * The used uri to execute the request.
   */
  uri: string;

  /**
   * List of headers that were sent to the server.
   */
  headers: Header[];
}

/**
 * Http response type that contains the basic
 * information about the received response.
 *
 * @interface
 */
declare interface HttpResponse {
  /**
   * Duration of the request in milliseconds.
   */
  callDuration: number;

  /**
   * The http status code that was returned.
   */
  statusCode: number;

  /**
   * If the response delivers content, it is read as string
   * and added here. If no content is returned,
   * this field is undefined.
   */
  content?: string;

  /**
   * The http reason phrase.
   */
  reason: string | null;

  /**
   * All headers that are returned by the questioned server.
   */
  headers: Header[];

  /**
   * Shortcut property for "return statusCode < 300".
   * Defines if the http request is asumed "successful".
   */
  isSuccess: boolean;
}

/**
 * Executor function for the given response test code.
 * Takes the function param and injects the request and the
 * response into it.
 *
 * @param func Testfunction. Takes the request and the response and must return a boolean value.
 */
declare function responseTest(func: (request: HttpRequest, response: HttpResponse) => boolean): void;

/**
 * Interface for logging purposes during tests.
 */
declare interface Logger {
  /**
   * Log the given messages or objects with the 'log' severity.
   *
   * @param messagesOrObjects Strings or objects to log. Objects are JSON.stringified.
   */
  log(...messagesOrObjects: any[]): void;

  /**
   * Log the given messages or objects with the 'info' severity.
   *
   * @param messagesOrObjects Strings or objects to log. Objects are JSON.stringified.
   */
  info(...messagesOrObjects: any[]): void;

  /**
   * Log the given messages or objects with the 'warn' severity.
   *
   * @param messagesOrObjects Strings or objects to log. Objects are JSON.stringified.
   */
  warn(...messagesOrObjects: any[]): void;

  /**
   * Log the given messages or objects with the 'error' severity.
   *
   * @param messagesOrObjects Strings or objects to log. Objects are JSON.stringified.
   */
  error(...messagesOrObjects: any[]): void;
}

/**
 * Logger instance.
 */
declare const logger: Logger;
