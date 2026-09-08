import {
  allocateCartOrders,
  getJigCapacity,
  getModelInputThickness,
  getModelJigType
} from "./cartUtils";

const machine = {
  jig1HeightMm: 150,
  jig2HeightMm: 200,
  jig3HeightMm: 120,
  jig4HeightMm: 100
};

function order(key, jigType, quantity, inputThickness = 10) {
  return {
    key,
    orderId: key,
    modelName: key,
    jigType,
    quantity,
    inputThickness
  };
}

describe("cart allocation", () => {
  it("calculates capacity by floor(jig height / input thickness)", () => {
    expect(getJigCapacity(machine, 1, 10)).toBe(15);
    expect(getJigCapacity(machine, 1, 0)).toBe(0);
  });

  it("spills the same jig order into the next cart position", () => {
    const result = allocateCartOrders([order("A", 1, 20)], machine);

    expect(result.computedOrders[0].placements.map((item) => [item.cartPositionIndex, item.quantity]))
      .toEqual([[1, 15], [2, 5]]);
    expect(result.positions[0].jigType).toBe(1);
    expect(result.positions[1].jigType).toBe(1);
  });

  it("moves a different jig order to the next position", () => {
    const result = allocateCartOrders([order("A", 1, 20), order("B", 2, 3)], machine);

    expect(result.computedOrders[1].placements[0].cartPositionIndex).toBe(3);
    expect(result.positions[1].jigType).toBe(1);
    expect(result.positions[2].jigType).toBe(2);
  });

  it("does not reuse a previous position after changing jig type", () => {
    const result = allocateCartOrders([
      order("A", 1, 1),
      order("B", 2, 1),
      order("C", 1, 1)
    ], machine);

    expect(result.computedOrders.map((item) => item.placements[0].cartPositionIndex))
      .toEqual([1, 2, 3]);
  });

  it("reports overflow beyond four positions", () => {
    const result = allocateCartOrders([
      order("A", 1, 15),
      order("B", 2, 20),
      order("C", 3, 12),
      order("D", 4, 10),
      order("E", 1, 1)
    ], machine);

    expect(result.errors.some((error) => error.includes("vượt quá 4 vị trí"))).toBe(true);
    expect(result.computedOrders[4].allocatedQuantity).toBe(0);
  });

  it("reads jig type from the model profile robot data", () => {
    expect(getModelJigType({ robotData: { jigSupplyType: 4 } })).toBe(4);
    expect(getModelJigType({ robotData: { jigSupplyType: 0 } })).toBe(0);
  });

  it("falls back to nested model values when top-level values are empty", () => {
    expect(getModelJigType({ jigType: 0, robotData: { jigSupplyType: 4 } })).toBe(4);
    expect(getModelInputThickness({ inputBlankThickness: 0, robotData: { inputBlankThickness: 12.5 } })).toBe(12.5);
  });
});
