import React, { Component, Fragment, useState } from 'react';

interface NameValueElement {
  name: string;
  value: string;
  webCheckId: number;
  id: number;
}

interface Props {
  data: NameValueElement[];
  enumerable: string;
  webCheckId: number;
}

interface InputProps {
  enumerable: string;
  index: number;
  property: string;
  value: string;
  hidden?: boolean;
  onChange?(value: string): void;
}

interface FormSetProps {
  enumerable: string;
  index: number;
  data: NameValueElement;
  onChange(value: NameValueElement): void;
  onDelete(): void;
}

interface State {
  data: NameValueElement[];
}

const ElementInput = ({ enumerable, index, property, value: initialValue, hidden, onChange }: InputProps) => {
  const [value, setValue] = useState(initialValue || '');

  return (
    <Fragment>
      <input
        type={hidden ? 'hidden' : 'text'}
        id={`WebCheck_${enumerable}_${index}__${property}`}
        name={`WebCheck.${enumerable}[${index}].${property}`}
        value={value}
        onChange={(e) => {
          setValue(e.target.value);
          if (onChange) {
            onChange(e.target.value);
          }
        }}
      />
      {
        value.trim() === '' && !!!hidden &&
        <span>This value is required</span>
      }
    </Fragment>
  );
};

const FormSet = ({ enumerable, index, data, onChange, onDelete }: FormSetProps) => (
  <div className="form-set">
    <ElementInput
      enumerable={enumerable}
      index={index}
      property="Id"
      hidden
      value={data.id.toString()}
    />
    <ElementInput
      enumerable={enumerable}
      index={index}
      property="WebCheckId"
      hidden
      value={data.webCheckId.toString()}
    />
    <div className="row">
      <div className="col-4">
        <label>Name</label>
      </div>
      <div className="col-8">
        <ElementInput
          enumerable={enumerable}
          index={index}
          property="Name"
          value={data.name}
          onChange={value => !!onChange && onChange({ ...data, name: value })}
        />
      </div>
    </div>
    <div className="row m-t-1">
      <div className="col-4">
        <label>Value</label>
      </div>
      <div className="col-8">
        <ElementInput
          enumerable={enumerable}
          index={index}
          property="Value"
          value={data.value}
          onChange={newValue => !!onChange && onChange({ ...data, value: newValue })}
        />
      </div>
    </div>
    <div className="row m-t-1">
      <div className="col text-right">
        <a onClick={() => onDelete()}>delete</a>
      </div>
    </div>
  </div>
);

export class WebcheckNameValueForm extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      data: props.data || [],
    };
  }

  public render(): JSX.Element {
    return (
      <Fragment>
        {
          this.state.data.map(
            (element, index) =>
              <FormSet
                key={`formset_${this.props.enumerable}_${index}_${this.state.data.length}`}
                data={element}
                index={index}
                enumerable={this.props.enumerable}
                onChange={(value) => {
                  const { data } = this.state;
                  data.splice(index, 1, value);
                  this.setState({
                    data,
                  });
                }}
                onDelete={() => {
                  const { data } = this.state;
                  data.splice(index, 1);
                  this.setState({
                    data,
                  });
                }}
              />,
          )
        }
        <div className="form-set">
          <div className="row">
            <div className="col text-right">
              <button
                className="btn secondary"
                type="button"
                onClick={() => {
                  const { data } = this.state;
                  data.push({ name: '', value: '', webCheckId: this.props.webCheckId, id: 0 });
                  this.setState({
                    data,
                  });
                }}
              >
                add
              </button>
            </div>
          </div>
        </div>
      </Fragment>
    );
  }
}
