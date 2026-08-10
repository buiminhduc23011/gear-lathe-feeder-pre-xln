import {
  MODEL_METADATA_FIELDS,
  buildModelFormValues,
  buildModelPayload
} from "./ModelProfiles";

describe("ModelProfiles model parameter helpers", () => {
  it("includes the D5560 and D5562/D5563 fields in the web metadata form", () => {
    expect(MODEL_METADATA_FIELDS).toEqual(expect.arrayContaining([
      expect.objectContaining({ key: "inputBlankDiameter", type: "real" }),
      expect.objectContaining({ key: "op2ChuckSleeveDepth", type: "real" })
    ]));
  });

  it("round-trips the new values through form defaults and API payload", () => {
    const values = buildModelFormValues({
      modelName: "MODEL-01",
      inputBlankDiameter: 42.125,
      op2ChuckSleeveDepth: 7.5,
      robotData: {}
    });

    expect(values).toMatchObject({
      inputBlankDiameter: 42.125,
      op2ChuckSleeveDepth: 7.5
    });

    expect(buildModelPayload(values, values.modelName)).toMatchObject({
      inputBlankDiameter: 42.125,
      op2ChuckSleeveDepth: 7.5
    });
  });

  it("defaults missing values for legacy model profiles to zero", () => {
    expect(buildModelFormValues(null)).toMatchObject({
      inputBlankDiameter: 0,
      op2ChuckSleeveDepth: 0
    });
  });
});
