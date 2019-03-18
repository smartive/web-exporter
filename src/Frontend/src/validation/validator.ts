export type Validator<T = string> = (value: T) => string | null;

export const isRequired: Validator = value =>
  !!value ? null : 'value is required';

export const isUrl: Validator = value =>
  /^(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+$/.test(
    value,
  )
    ? null
    : 'value must be a valid url';
