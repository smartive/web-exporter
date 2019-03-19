import { Base } from './base';
import { HttpMethod } from './http-method';
import { Label } from './label';
import { RequestHeader } from './request-header';
import { ResponseTest } from './response-test';

export interface WebCheck extends Base {
  name: string;
  url: string;
  method: HttpMethod;
  lastExecution: Date;
  lastResult: boolean;
  failureReason: string;
  requestHeaders: RequestHeader[];
  labels: Label[];
  reponseTests: ResponseTest[];
}
